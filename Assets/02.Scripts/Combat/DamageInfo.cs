using UnityEngine;

public struct DamageInfo
{
    public float damage;              // HP 피해량
    public float infection;           // 감염도 증가량
    public Vector3 hitDirection;      // 넉백 방향 (때린 쪽 → 맞은 쪽)
    public float knockbackDistance;   // 0이면 넉백 없음
}