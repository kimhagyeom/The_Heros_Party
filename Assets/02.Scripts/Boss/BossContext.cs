using UnityEngine;
using System;
public class BossContext
{
    public Transform target;
    public LayerMask targetMask;
    public AttackTelegraph telegraph;
    public Health bossHealth;
    public Action<BossPattern> queuePattern;
}
