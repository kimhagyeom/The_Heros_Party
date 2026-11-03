using System.Collections;
using UnityEngine;

public class PushPattern : BossPattern
{
    PushData D => (PushData)data;

    protected override IEnumerator Execute(BossContext ctx)
    {
        if (!ctx.target.TryGetComponent(out PlayerController player)) yield break;

        yield return new WaitForSeconds(D.warningTime);

        // 보스 → 플레이어 방향 = 가장 가까운 외곽 쪽
        Vector3 dir = ctx.target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;

        player.Knockback(dir, D.Push_Distance, D.Push_Speed);
        player.Stun(D.Stun_Duration);

        // 밀려나는 동안 대기
        yield return new WaitForSeconds(D.Push_Speed > 0f ? D.Push_Distance / D.Push_Speed : 0f);
    }
}