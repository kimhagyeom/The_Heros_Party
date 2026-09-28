using UnityEngine;
using UnityEngine.Serialization;


public enum EnemyType { Normal, MiniBoss , Boss }
//데이터 시트 : 적 Enum
public enum Enemy_Atk_Type { Melee, Projectile, Aoe }
public enum Enemy_Hitbox_Shape { Sector, Box }
public enum Special_Ability { None }

//데이터 시트 : 적 데이터 시트
[CreateAssetMenu(menuName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("공통")]
    public EnemyType enemyType;
    public int enemy_ID;
    [FormerlySerializedAs("enemyName")] public string enemy_Name;
    [FormerlySerializedAs("maxHP")] public float max_HP;
    [FormerlySerializedAs("moveSpeed")] public float move_Speed;

    [Header("경직 / 넉백")]
    public bool can_Stagger;
    public float stagger_Duration;
    public bool can_Knockback;
    public float knockback_Resistance;

    [Header("이동 / 충돌")]
    public float collision_Radius;
    public float stop_Distance;

    [Header("특수")]
    public Special_Ability special_Ability;

    [Header("공격 목록")]
    public EnemyAtkData[] atk_List;
}
