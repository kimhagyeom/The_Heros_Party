using UnityEngine;
using System;
public class BossContext
{
    public Transform target;
    public LayerMask targetMask;
    public AttackTelegraph telegraph;
    public Health bossHealth;
    public Action<BossPattern> queuePattern;
    public Action<float, float> requestStun; //기절 시간, 받는 피해 배율
}
