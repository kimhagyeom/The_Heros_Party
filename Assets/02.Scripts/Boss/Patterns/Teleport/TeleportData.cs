using UnityEngine;

[CreateAssetMenu(menuName = "Boss/Patterns/TeleportData")]
public class TeleportData : BossPatternData
{
    [Header("발동 조건")]
    public float triggerDistance = 8f;     // 플레이어가 이보다 멀면 사용

    [Header("이동 위치")]
    public float minDistance = 3f;         // 플레이어로부터 최소 거리 (바로 옆 방지)
    public float maxDistance = 5f;         // 플레이어로부터 최대 거리
    public int maxPickTries = 10;
}
