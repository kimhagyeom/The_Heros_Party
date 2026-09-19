using System.Collections.Generic;
using UnityEngine;


public enum AttackType { Melee, Ranged }
public enum EnemyType { Normal, MiniBoss , Boss }

[CreateAssetMenu(menuName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("공통")]
    public EnemyType enemyType;
    public string enemyName;
    public int maxHP;
    public float moveSpeed;
}
