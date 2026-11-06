using System.Collections;
using UnityEngine;

// 2페이즈 보호막 기믹: 밀쳐내기 → (감염 안개) → 중앙 이동 → 보호막 + 흙벽 → 결과
public class PhaseGimmickPattern : BossPattern
{
    [SerializeField] private PushPattern push;
    [SerializeField] private ShieldPattern shield;
    [SerializeField] private WallPattern wall;
    [SerializeField] private bool repeatFullOnSecond = true; // 2차 트리거에서도 밀쳐내기~이동까지 할지
    [SerializeField] private InfectionFog fog;

    public int TriggerCount { get; set; } // 컨트롤러가 몇 번째 트리거인지 알려줌
    public float[] HpTriggers => shield != null ? shield.HpTriggers : new float[0];

    protected override IEnumerator Execute(BossContext ctx)
    {
        bool full = TriggerCount <= 1 || repeatFullOnSecond;
        if (full)
        {
            //밀쳐내기
            yield return RunAndWait(push, ctx);

            //감염 안개
            if (fog != null) fog.Begin(ctx.target);

            //중앙 이동 (추가)
        }

        //보호막,흙벽 동시 진행
        shield.Run(ctx, null);
        if (wall != null) wall.Run(ctx, null);
        while (shield.IsRunning) yield return null;
        //결과는 ShieldPattern이 처리 (성공: 기절 요청 / 실패: 무적 + 메테오 예약)
        if (wall != null) wall.Stop(); // 보호막이 끝나면 남은 벽 정리
    }

    IEnumerator RunAndWait(BossPattern pattern, BossContext ctx)
    {
        if (pattern == null) yield break;
        pattern.Run(ctx, null);
        while (pattern.IsRunning) yield return null;
    }

    protected override void OnStopped()
    {
        if (push != null) push.Stop();
        if (shield != null) shield.Stop();
        if (wall != null) wall.Stop();
    }
    void OnDestroy()
    {
        if (fog != null) fog.End();
    }
}