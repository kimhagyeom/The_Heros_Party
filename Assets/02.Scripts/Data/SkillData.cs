using UnityEngine;

[CreateAssetMenu(menuName = "SkillData")]
public class SkillData : ScriptableObject
{
    [Header("기본 정보")]
    public int skill_ID;
    public string skill_Name;
    public int base_Skill_ID;          // 강화 스킬일 때 원본 스킬 ID (없으면 0)
    public AttackType atk_Type;        // Melee / Projectile / Aoe
    public bool has_Target;            // 타겟 지정 여부 (일섬)

    [Header("피해 / 쿨타임")]
    public float dmg_Rate;             // 기본 공격력 × 배율
    public float cooldown;
    public int max_Stacks = 1;         // 최대 충전량 (일섬 2)

    [Header("타이밍")]
    public float windup_Time;          // 선딜레이
    public float active_Time;          // 판정 지속시간 / 투사체 수명
    public float recovery_Time;        // 후딜레이

    [Header("판정 형태")]
    public HitboxShape hitbox_Shape;
    public float range;                // 부채꼴 반지름
    public float angle;
    public float width;
    public float length;

    [Header("이동")]
    public MoveType move_Type;
    public float move_Distance;
    public float move_Time;
    public bool has_Invincibility;

    [Header("투사체")]
    public bool projectile_Pierce;
    public float projectile_Speed;
    public int projectile_Count;
    public float spread_Angle;         // 확산각

    [Header("기타")]
    public GameObject projectilePrefab; // 아직 시트엔 x
    public GameObject effectPrefab;
}
