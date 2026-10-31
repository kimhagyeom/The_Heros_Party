using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class PhaseInfo
{
    public float hpRatio;              // 이 페이즈가 시작되는 체력 비율
    public List<BossPattern> patterns; // 이 페이즈에서 쓰는 패턴
    public BossPattern entryPhasePattern;
}
public enum BossState { Idle, Pattern, Stunned, Dead }
public class BossController : Enemy
{    
    [SerializeField] List<PhaseInfo> phases;
    [SerializeField] float idleTime;
    [SerializeField] float retryDelay = 0.2f;
    [SerializeField] private AttackTelegraph telegraph;

    BossContext ctx;
    BossPattern current; 
    BossPattern pendingPattern;
    public void QueuePattern(BossPattern pattern) => pendingPattern = pattern;
    int phaseIndex;

    BossState state = BossState.Idle;
    Coroutine waitRoutine;
    Coroutine stunRoutine;

    public BossState State => state;

    bool started; //보스 방 입장
    public bool IsInBattle => started && !IsDead;

    public override void Start()
    {
        base.Start();
        health.OnDamaged += HandleDamaged;
        health.OnDied += HandleDied;
    }

    public void StartBattle() // 보스 방 입장 시 Room이 호출
    {
        if (started) return;
        var player = FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            Debug.LogWarning("BossController : PlayerController를 찾지 못함");
            return;
        }
        started = true;
        Init(player.transform);
    }

    void Init(Transform target)
    {
        ctx = new BossContext
        {                                        
            target = target,
            targetMask = 1 << target.gameObject.layer, // 공격 안맞으면 이 부분 확인
            telegraph = telegraph,
            bossHealth = health,
            queuePattern = QueuePattern,
        };
        phaseIndex = 0;
        PickNext();
    }
    private void PickNext()
    {
        if (state == BossState.Dead || state == BossState.Stunned) return;

        if (pendingPattern != null)
        {
            current = pendingPattern;
            pendingPattern = null;
            state = BossState.Pattern;
            current.Run(ctx, OnPatternEnd);
            return;
        }
        List<BossPattern> candidates = phases[phaseIndex].patterns.FindAll(p=> p != null && p.CanUse());
        if(candidates.Count == 0) 
        {
            waitRoutine = StartCoroutine(WaitThenPick(retryDelay));
            return;
        }
        current = candidates[Random.Range(0, candidates.Count)];

        Debug.Log($"[Boss] 패턴 실행: {current.name} ({phaseIndex + 1}페이즈)");

        state = BossState.Pattern;
        current.Run(ctx, OnPatternEnd);
    }
    private IEnumerator WaitThenPick(float delay)
    {
        yield return new WaitForSeconds(delay);
        waitRoutine = null;
        PickNext();
    }
    private void OnPatternEnd() // current 비우고 PickNext
    {
        current = null;
        state = BossState.Idle;
        waitRoutine = StartCoroutine(WaitThenPick(idleTime));
    }

    // 진행 중인 패턴과 대기 코루틴을 모두 끊음
    void InterruptCurrent()
    {
        if (waitRoutine != null) { StopCoroutine(waitRoutine); waitRoutine = null; }
        if (current != null) { current.Stop(); current = null; }
        if (telegraph != null) telegraph.Hide(); // 끊긴 패턴의 경고 표시 정리
    }

    public void OnHpChanged(float ratio) // 추후 Hp 처리에서 사용 예정
    {
        if (!started || state == BossState.Dead) return;
        int next = phaseIndex + 1;
        if (next < phases.Count && ratio <= phases[next].hpRatio)
        {
            phaseIndex = next;
            pendingPattern = phases[phaseIndex].entryPhasePattern;
            Debug.Log($"[Boss] {phaseIndex + 1}페이즈 진입 (HP {ratio:P0}), 진입 패턴: {(pendingPattern != null ? pendingPattern.name : "없음")}");
            if (state == BossState.Stunned) return; // 기절 끝나면 pendingPattern부터 실행됨
            InterruptCurrent();
            state = BossState.Idle;
            PickNext();
        }
    }
    void HandleDamaged(DamageInfo info)
    {
        OnHpChanged(health.Current / health.Max); 
        //Debug.Log(health.Current / health.Max);
    }
    public void Stun(float duration) // 보호막 파괴 성공 시 사용
    {
        if (state == BossState.Dead) return;
        InterruptCurrent();
        if (stunRoutine != null) StopCoroutine(stunRoutine);
        stunRoutine = StartCoroutine(StunRoutine(duration));
    }
    IEnumerator StunRoutine(float duration)
    {
        state = BossState.Stunned;
        yield return new WaitForSeconds(duration);
        stunRoutine = null;
        state = BossState.Idle;
        PickNext();
    }

    void HandleDied()
    {
        InterruptCurrent();
        if (stunRoutine != null) StopCoroutine(stunRoutine);
        state = BossState.Dead;
    }

    public override void OnHit(DamageInfo info) { } // 보스는 경직,넉백 없음
}
