using System.Collections;
using UnityEngine;

public class ShieldPattern : BossPattern
{
    [SerializeField] BossPattern failPattern; 
    ShieldData D => (ShieldData) data;
    Health health;
    protected override IEnumerator Execute(BossContext ctx)
    {
        health = ctx.bossHealth;

        // 보호막 켜기
        health.SetShield(D.activation_Count);

        // 제한시간 동안 기다리기 (깨지면 바로 빠져나옴)
        float elapsed = 0f;
        while (elapsed < D.shield_Duration && health.IsShielded)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (!health.IsShielded)
        {
            health.SetDamageMultiplier(D.Vulnerable_Multiplier);
            //기절
            yield return new WaitForSeconds(D.Break_Stun_Duration);
            health.SetDamageMultiplier(1f);
        }
        else
        {
            // 실패 → 보호막 끄고, 무적, 다음 패턴으로 메테오 예약
            health.SetShield(0f);
            health.AddInvincible(D.Fail_Invincible_Duration);
            ctx.queuePattern?.Invoke(failPattern);
        }
        yield return null;
    }
    protected override void OnStopped()
    {
        if (health == null) return;
        health.SetShield(0f);
        health.SetDamageMultiplier(1f);
    }
    void HandleShieldBroken()
    {
        
    }
}
