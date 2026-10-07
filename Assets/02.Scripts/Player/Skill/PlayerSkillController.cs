using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private Color targetMarkerColor = new Color(1f, 0.85f, 0.2f); // 일섬 대상 미리 표시 색
    [SerializeField] private float targetMarkerRadius = 0.7f;

    private PlayerController playerController;
    private AttackTelegraph pathTelegraph;   // 일섬 경로 표시
    private AttackTelegraph targetMarker;    // 일섬 대상 미리 표시
    private AttackTelegraph shownTelegraph;  // 지금 보이는 범위(경로)
    private Coroutine hideTelegraphRoutine;
    private SkillExecutor skillExecutor;
    private SkillSlot[] slots;
    private InputAction[] skillActions;

    public bool IsCasting { get; private set; }
    public SkillPhase Phase { get; private set; }
    public int CastingSlot { get; private set; } = -1;

    public SkillSlot GetSlot(int index) => slots != null && index >= 0 && index < slots.Length ? slots[index] : null;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        skillExecutor = GetComponent<SkillExecutor>();

        if (telegraph == null)
            telegraph = AttackTelegraph.Create(transform, telegraphColor);
        pathTelegraph = AttackTelegraph.Create(null, telegraphColor);
        targetMarker = AttackTelegraph.Create(null, targetMarkerColor);

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
        UpdateTargetMarker();
    }

    // 일섬? : 대상 지정 스킬을 지금 쓸 수 있으면, 누르면 잡힐 적 발밑에 원 표시
    void UpdateTargetMarker()
    {
        Enemy target = null;
        if (!playerController.IsBusy)
        {
            foreach (SkillSlot slot in slots)
            {
                if (slot.IsEmpty || !slot.data.has_Target || slot.stacks <= 0) continue;
                target = skillExecutor.FindTarget(playerController, slot.data);
                break;
            }
        }

        if (target != null)
            targetMarker.ShowCircleAt(target.transform.position, targetMarkerRadius);
        else
            targetMarker.Hide();
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

        // 대상 지정 스킬은 충전을 쓰기 전에 대상부터 확인 (없으면 스킬 안 나가고 충전도 그대로)
        Enemy target = null;
        if (slot.data.has_Target)
        {
            target = skillExecutor.FindTarget(playerController, slot.data);
            if (target == null)
            {
                Debug.Log($"[{slot.data.skill_Name}] 대상 없음 ({slot.data.range}m 안에 적이 없음)");
                return false;
            }
            playerController.FaceDirection(target.transform.position - transform.position);
        }

        if (slot.IsFull)
            slot.rechargeRemaining = slot.data.cooldown;
        slot.stacks--;

        StartCoroutine(CastRoutine(index, slot.data, target));
        return true;
    }

    IEnumerator CastRoutine(int index, SkillData data, Enemy target)
    {
        IsCasting = true;
        CastingSlot = index;

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
