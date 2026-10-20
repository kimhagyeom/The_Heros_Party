using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static Thry.ThryEditor.AnimationParser;

public enum SkillPhase { None, Windup, Active, Recovery } // 선딜 / 판정 / 후딜

public class SkillSlot
{
    public SkillData data;
    public int stacks;               // 지금 쓸 수 있는 횟수
    public float rechargeRemaining;  // 다음 스택 충전까지 남은 시간

    public SkillSlot(SkillData data)
    {
        this.data = data;
        stacks = MaxStacks; // 처음엔 가득 찬 상태로 시작
    }

    public bool IsEmpty => data == null;
    public int MaxStacks => data != null ? Mathf.Max(1, data.max_Stacks) : 0;
    public bool IsFull => stacks >= MaxStacks;
}

// 판정 순간의 실제 효과(범위 판정, 투사체, 이동)는 SkillExecutor가 담당
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(SkillExecutor))]
public class PlayerSkillController : MonoBehaviour
{
    public const int SlotCount = 4;

    [SerializeField] private SkillData[] slotSkills = new SkillData[SlotCount];

    [Header("판정 범위 표시")]
    [SerializeField] private AttackTelegraph telegraph;
    [SerializeField] private Color telegraphColor = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private float telegraphLingerTime = 0f;
    [SerializeField] private Material telegraphMaterial;
    [Header("대상 지정")]
    [SerializeField] private float hoverRadius = 1.2f; // 커서에서 이 반경 안에 있는 적만 대상으로 잡음

    private PlayerController playerController;
    private AttackTelegraph pathTelegraph;   // 일섬 경로 표시
    private AttackTelegraph shownTelegraph;  // 지금 보이는 범위(경로)
    private Coroutine hideTelegraphRoutine;
    private SkillExecutor skillExecutor;
    private SkillSlot[] slots;
    private InputAction[] skillActions;
    private Animator animator;

    public bool IsCasting { get; private set; }
    public SkillPhase Phase { get; private set; }
    public int CastingSlot { get; private set; } = -1;

    public SkillSlot GetSlot(int index) => slots != null && index >= 0 && index < slots.Length ? slots[index] : null;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        skillExecutor = GetComponent<SkillExecutor>();
        animator = GetComponentInChildren<Animator>();

        if (telegraph == null)
            telegraph = AttackTelegraph.Create(transform, telegraphColor, telegraphMaterial);
        pathTelegraph = AttackTelegraph.Create(null, telegraphColor, telegraphMaterial);

        slots = new SkillSlot[SlotCount];
        for (int i = 0; i < SlotCount; i++)
        {
            SkillData data = i < slotSkills.Length ? slotSkills[i] : null;
            slots[i] = new SkillSlot(data);
        }
    }

    void Start()
    {
        // InputActions는 PlayerController.Awake에서 만들어지므로 Start에서 구독
        var player = playerController.InputActions.Player;
        skillActions = new InputAction[] { player.Skill_1, player.Skill_2, player.Skill_3, player.Skill_4 };

        foreach (InputAction action in skillActions)
            action.performed += OnSkillPerformed;
    }

    void OnDisable()
    {
        if (skillActions == null) return;

        foreach (InputAction action in skillActions)
            action.performed -= OnSkillPerformed;
    }

    void Update()
    {
        RechargeSlots();
    }

    private void OnSkillPerformed(InputAction.CallbackContext ctx)
    {
        int index = Array.IndexOf(skillActions, ctx.action);
        if (index >= 0)
            TryUseSkill(index);
    }

    // 덜 찬 슬롯은 cooldown초마다 스택 1개씩 충전
    void RechargeSlots()
    {
        foreach (SkillSlot slot in slots)
        {
            if (slot.IsEmpty || slot.IsFull) continue;

            slot.rechargeRemaining -= Time.deltaTime;
            if (slot.rechargeRemaining <= 0f)
            {
                slot.stacks++;
                slot.rechargeRemaining = slot.IsFull ? 0f : slot.data.cooldown;
            }
        }
    }

    public bool TryUseSkill(int index)
    {
        SkillSlot slot = GetSlot(index);
        if (slot == null || slot.IsEmpty)
        {
            Debug.Log($"[{index + 1}]번 슬롯에 스킬이 없음");
            return false;
        }

        if (playerController.IsBusy) return false;
        if (slot.stacks <= 0) return false;

        // 대상 지정 스킬(일섬): 커서 근처 + 사거리 안에 적이 있을 때만 바로 시전
        Enemy target = null;
        if (slot.data.has_Target)
        {
            //target = skillExecutor.FindTargetInRange(playerController, slot.data, hoverRadius);
            //감지 범위 미설정 추후 수정 예정
            target = skillExecutor.FindBestEnemy(playerController, 0.6f);
            if (target == null)
            {
                Debug.Log($"[{slot.data.skill_Name}] 커서 근처 사거리 안에 대상 없음");
                return false;
            }
        }

        StartCast(index, target);
        return true;
    }

    void StartCast(int index, Enemy target)
    {
        SkillSlot slot = slots[index];

        if (target != null)
            playerController.FaceDirection(target.transform.position - transform.position);
        else if (slot.data.atk_Type == AttackType.Projectile && playerController.TryGetMouseWorldPosition(out Vector3 mousePos))
            playerController.FaceDirection(mousePos - transform.position); // 투사체(단검 쇄도): 커서 방향으로 발사

        if (slot.IsFull)
            slot.rechargeRemaining = slot.data.cooldown;
        slot.stacks--;

        StartCoroutine(CastRoutine(index, slot.data, target));
    }
    public void ResetSkills() //리셋 시 스킬 끊고 스택 초기화 , 코루틴도 정지
    {
        StopAllCoroutines();
        skillExecutor.StopAllCoroutines();
        hideTelegraphRoutine = null;

        telegraph.Hide();
        pathTelegraph.Hide();

        IsCasting = false;
        Phase = SkillPhase.None;
        CastingSlot = -1;

        foreach (SkillSlot slot in slots)
        {
            if (slot.IsEmpty) continue;
            slot.stacks = slot.MaxStacks;
            slot.rechargeRemaining = 0f;
        }
    }
    IEnumerator CastRoutine(int index, SkillData data, Enemy target)
    {
        IsCasting = true;
        CastingSlot = index;
        PlaySkillAnimation(data);

        // 선딜: 범위 표시 후 안쪽이 차오름
        SetPhase(SkillPhase.Windup, data);
        bool hasTelegraph = ShowTelegraph(data, target);
        float elapsed = 0f;
        while (elapsed < data.windup_Time)
        {
            elapsed += Time.deltaTime;
            if (hasTelegraph) shownTelegraph.SetProgress(elapsed / data.windup_Time);
            yield return null;
        }

        // 판정: 범위 꽉 찬 상태로 유지
        SetPhase(SkillPhase.Active, data);
        if (hasTelegraph) shownTelegraph.SetProgress(1f);
        skillExecutor.Execute(playerController, data, target);
        if (data.atk_Type != AttackType.Projectile && data.active_Time > 0f)
            yield return new WaitForSeconds(data.active_Time);

        // 후딜: 범위 숨김
        SetPhase(SkillPhase.Recovery, data);
        if (hasTelegraph) HideTelegraph();
        if (data.recovery_Time > 0f)
            yield return new WaitForSeconds(data.recovery_Time);

        SetPhase(SkillPhase.None, data);
        IsCasting = false;
        CastingSlot = -1;
    }

    void PlaySkillAnimation(SkillData data)
    {
        if (animator == null) return;
        int stateHash = Animator.StringToHash($"Player_Skill{data.skill_ID}");
        if (animator.HasState(0, stateHash))
        {
            animator.Play(stateHash, 0, 0f);
        }
    }

    //범위
    bool ShowTelegraph(SkillData data, Enemy target)
    {
        bool isSector = data.atk_Type == AttackType.Melee && data.hitbox_Shape == HitboxShape.Sector;
        bool isTargetPath = data.move_Type == MoveType.Target_Back && target != null;

        AttackTelegraph next = isSector ? telegraph : isTargetPath ? pathTelegraph : null;
        if (next == null) return false;

        // 이전 스킬 범위가 아직 남아있으면 바로 숨기고 새로 그림
        if (hideTelegraphRoutine != null)
        {
            StopCoroutine(hideTelegraphRoutine);
            hideTelegraphRoutine = null;
            shownTelegraph.Hide();
        }

        shownTelegraph = next;
        if (isSector)
            telegraph.ShowSector(data.range, data.angle);
        else
            pathTelegraph.ShowBoxAt(transform.position, transform.rotation, data.width,
                skillExecutor.GetPathLength(transform, target, data));
        return true;
    }

    void HideTelegraph()
    {
        if (telegraphLingerTime > 0f)
            hideTelegraphRoutine = StartCoroutine(HideTelegraphAfter(shownTelegraph, telegraphLingerTime));
        else
            shownTelegraph.Hide();
    }

    IEnumerator HideTelegraphAfter(AttackTelegraph target, float delay)
    {
        yield return new WaitForSeconds(delay);
        target.Hide();
        hideTelegraphRoutine = null;
    }

    void SetPhase(SkillPhase phase, SkillData data)
    {
        Phase = phase;
        Debug.Log($"[{data.skill_Name}] {GetPhaseName(phase)}");
    }

    public static string GetPhaseName(SkillPhase phase)
    {
        switch (phase)
        {
            case SkillPhase.Windup: return "선딜";
            case SkillPhase.Active: return "판정";
            case SkillPhase.Recovery: return "후딜";
            default: return "종료";
        }
    }
}
