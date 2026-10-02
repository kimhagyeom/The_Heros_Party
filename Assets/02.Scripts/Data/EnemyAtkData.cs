using UnityEngine;

[CreateAssetMenu(menuName = "EnemyAtkData")]
public class EnemyAtkData : ScriptableObject
{
    [Header("기본 정보")]
    public int atk_ID;
    public int enemy_ID;
    public string atk_Name;
    public AttackType atk_Type;

    [Header("피해")]
    public float hp_Damage;
    public float infection_Value;

    [Header("거리 / 타이밍")]
    public float atk_Range;
    public float windup_Time;
    public float active_Time;
    public float recovery_Time;
    public float cooldown;

    [Header("판정 형태")]
    public HitboxShape hitbox_Shape;
    public float radius;
    public float angle;
    public float width;
    public float length;

    [Header("부가 효과")]
    public bool has_Super_Armor;
    public bool has_Knockback;
    public float knockback_Distance;
    public bool has_Slow;
    public float slow_Time;
    public float slow_Per;
    [Header("기타")]
    public Projectile projectilePrefab;
}
