using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum SkillPhase { None, Windup, Active, Recovery } // 선딜 / 판정 / 후딜

public class SkillSlot
{
    public SkillData data;           // ★ 복사본
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

    [Tooltip("원본 SkillData (인스펙터 연결). 실제 사용은 slots[i].data 복사본")]
    [SerializeField] public SkillData[] slotSkills = new SkillData[SlotCount];
    [SerializeField] public SkillSlotUI[] skillSlotUIs = new SkillSlotUI[SlotCount];

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
    public bool IsCancelable { get; private set; }
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

        // 저장 시스템 없이 테스트할 때를 위한 기본 세팅 (인스펙터 원본으로 복사본 생성)
        InitSkills(slotSkills);
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

    // ─────────────────────────────────────────────
    // 복사본 생성 / 찾기 / 저장
    // ─────────────────────────────────────────────

    // 런 시작 / 이어하기 때 호출: 원본 → 복사본
    public void InitSkills(SkillData[] originals)
    {
        if (slots == null) slots = new SkillSlot[SlotCount];

        for (int i = 0; i < SlotCount; i++)
        {
            SkillData original = originals != null && i < originals.Length ? originals[i] : null;
            if (i < slotSkills.Length) slotSkills[i] = original;
            SetSlot(i, original);
        }
    }

    // 슬롯 새로 만들기: 이전 복사본 정리 → 새 복사본 → UI 연결
    void SetSlot(int index, SkillData original)
    {
        if (slots[index] != null && slots[index].data != null)
            Destroy(slots[index].data);

        SkillData copy = original != null ? Instantiate(original) : null;
        slots[index] = new SkillSlot(copy);

        if (index < skillSlotUIs.Length && skillSlotUIs[index] != null)
            skillSlotUIs[index].Bind(slots[index]);
    }

    // 스킬 ID로 복사본 찾기 (UpgradeManager, UpgradeSelector에서 사용)
    public SkillData FindSkill(int skillId)
    {
        if (slots == null) return null;
        foreach (SkillSlot slot in slots)
        {
            if (!slot.IsEmpty && slot.data.skill_ID == skillId)
                return slot.data;
        }
        return null;
    }

    public int[] GetSkillIds()
    {
        int[] ids = new int[SlotCount];
        for (int i = 0; i < SlotCount; i++)
            ids[i] = slots[i].IsEmpty ? 0 : slots[i].data.skill_ID;
        return ids;
    }

    public List<SkillSlotSave> GetSlotSaves()
    {
        List<SkillSlotSave> list = new List<SkillSlotSave>();
        foreach (SkillSlot slot in slots)
            list.Add(new SkillSlotSave { stacks = slot.stacks, rechargeRemaining = slot.rechargeRemaining });
        return list;
    }

    // 강화 Replay 뒤에 호출 (최대 충전량이 강화로 바뀌니까)
    public void RestoreSlots(List<SkillSlotSave> saves)
    {
        if (saves == null) return;
        for (int i = 0; i < SlotCount && i < saves.Count; i++)
        {
            if (slots[i].IsEmpty) continue;
            slots[i].stacks = Mathf.Clamp(saves[i].stacks, 0, slots[i].MaxStacks);
            slots[i].rechargeRemaining = saves[i].rechargeRemaining;
        }
    }

    // ─────────────────────────────────────────────
    // 쿨타임 / 사용
    // ─────────────────────────────────────────────

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

    //베이스캠프: 스킬 교체 (skill은 원본)
    public void SetSkill(int slotIndex, SkillData skill)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount) return;
        if (IsCasting)
        {
            Debug.Log("[베이스캠프] 스킬 시전 중에는 교체할 수 없음");
            return;
        }

        // 같은 스킬이 다른 슬롯에 있으면 그 슬롯은 비움 (중복 장착 방지) — 복사본이라 ID로 비교
        if (skill != null)
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (i == slotIndex) continue;
                if (!slots[i].IsEmpty && slots[i].data.skill_ID == skill.skill_ID)
                {
                    if (i < slotSkills.Length) slotSkills[i] = null;
                    SetSlot(i, null);
                    Debug.Log($"[베이스캠프] {i + 1}번 슬롯에 있던 {skill.skill_Name} 빠짐");
                }
            }
        }

        if (slotIndex < slotSkills.Length)
            slotSkills[slotIndex] = skill;
        SetSlot(slotIndex, skill);   // 새 복사본 → 스택 가득 찬 상태로 시작

        string name = skill != null ? skill.skill_Name : "(비움)";
        Debug.Log($"[베이스캠프] {slotIndex + 1}번 슬롯 → {name}");
    }

    // ─────────────────────────────────────────────
    // 시전 (기존 그대로)
    // ─────────────────────────────────────────────

    IEnumerator CastRoutine(int index, SkillData data, Enemy target)
    {
        Debug.Log("Cast!");
        IsCasting = true;
        CastingSlot = index;
        PlaySkillAnimation(data);

        // 선딜: 범위 표시 후 안쪽이 차오름
       SetPhase(SkillPhase.Windup, data);
        IsCancelable = true;                    // 선딜 시작: 대시로 취소 가능
        bool hasTelegraph = ShowTelegraph(data, target);
        float elapsed = 0f;
        while (elapsed < data.windup_Time)
        {
            elapsed += Time.deltaTime;
            if (playerController.isDash)
            {
                IsCancelable = false;
                CancelCast(data);               // 정리는 여기서 전부
                Debug.Log("Cast Canceled!");
                yield break;                    // break 대신: Execute까지 내려가지 않음
            }
            if (hasTelegraph) shownTelegraph.SetProgress(elapsed / data.windup_Time);
            yield return null;
        }
        IsCancelable = false;                   // 선딜 끝: 이제부터 취소 불가

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
    void CancelCast(SkillData data)
    {
        IsCasting = false;
        SetPhase(SkillPhase.None,data);
        CastingSlot = -1;
        HideTelegraph();
    }
    void PlaySkillAnimation(SkillData data)
    {
        if (animator == null) return;
        // ★ 스킬 ID가 10001 등으로 바뀌어서 애니메이터 상태 이름도 Player_Skill10001 형태로 맞춰야 함
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