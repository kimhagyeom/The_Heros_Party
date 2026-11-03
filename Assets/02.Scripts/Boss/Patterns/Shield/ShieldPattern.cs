using System.Collections;
using UnityEngine;

public class ShieldPattern : BossPattern
{
    [SerializeField] BossPattern failPattern;
    ShieldData D => (ShieldData)data;
    Health health;
    // 보스컨트롤러가 HP 트리거 확인에 사용
    public float[] HpTriggers => data == null ? new float[0] : new[] { D.trigger_HP1, D.trigger_HP2 };
    protected override IEnumerator Execute(BossContext ctx)
    {
        health = ctx.bossHealth;

        // 보호막 켜기
        health.SetShield(D.activation_Count);

        // 제한시간 동안 기다리기 (깨지면 바로 빠져나옴)
        float elapsed = 0f;
        while (elapsed < D.shield_Duration)
        {
            if (!health.IsShielded && D.End_On_Break) break;
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (!health.IsShielded)
        {
            // 성공 → 패턴이 끝난 뒤 컨트롤러가 기절 + 취약 처리
            ctx.requestStun?.Invoke(D.Break_Stun_Duration, D.Vulnerable_Multiplier);

            // health.SetDamageMultiplier(D.Vulnerable_Multiplier);
            // //기절
            // yield return new WaitForSeconds(D.Break_Stun_Duration);
            // health.SetDamageMultiplier(1f);
        }
        else
        {
            // 실패 → 보호막 끄고, 무적, 다음 패턴으로 메테오 예약
            health.SetShield(0f);
            health.AddInvincible(D.Fail_Invincible_Duration);
            ctx.queuePattern?.Invoke(failPattern);
        }
    }
    protected override void OnStopped()
    {
        if (health == null) return;
        health.SetShield(0f);
    }
    void HandleShieldBroken()
    {

    }
}
