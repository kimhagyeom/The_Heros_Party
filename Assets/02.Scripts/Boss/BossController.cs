using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;    
using Random = UnityEngine.Random;

[Serializable]
public class PhaseInfo
{
    public float hpRatio;              // 이 페이즈가 시작되는 체력 비율
    public List<BossPattern> patterns; // 이 페이즈에서 쓰는 패턴
    public BossPattern entryPhasePattern;
}
public enum BossState { Idle, Chase , Pattern, Stunned, Dead }
public class BossController : Enemy
{    
    [SerializeField] List<PhaseInfo> phases;
    [SerializeField] float idleTime;
    [SerializeField] float retryDelay = 0.2f;
    [SerializeField] private AttackTelegraph telegraph;
    [SerializeField] private PhaseGimmickPattern gimmickPattern;
    // 시아 범위
    [SerializeField] float visualRange;
    int shieldTriggerIndex; //발동한 보호막 트리거 수
    float pendingStunDuration; //패턴 종료 휴 기절
    float pendingStunMultiplier = 1f;

    public void RequestStun(float duration, float multiplier)
    {
        pendingStunDuration = duration;
        pendingStunMultiplier = multiplier;
    }

    public BossContext ctx;
    BossPattern current; 
    BossPattern pendingPattern;
    
    private Animator animator;
    //EnemyMovement movement;
    NavMeshAgent agent;
    public void QueuePattern(BossPattern pattern) => pendingPattern = pattern;
    int phaseIndex;

    BossState state = BossState.Idle;
    Coroutine waitRoutine;
    Coroutine stunRoutine;

    public BossState State => state;

    bool started; //보스 방 입장
    public bool IsInBattle => started && !IsDead;

    void Update()
    {
        switch (state)
        {
            case BossState.Chase:
                UpdateChase();
                break;
        }
    }

    public override void Start()
    {
        base.Start();
       // movement = GetComponent<EnemyMovement>();
        agent = GetComponent<NavMeshAgent>();
        health.OnDamaged += HandleDamaged;
        health.OnDied += HandleDied;
        animator = GetComponentInChildren<Animator>();
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
            requestStun = RequestStun,
            //boss = transform,
        };
        phaseIndex = 0;
        shieldTriggerIndex = 0;
        PickNext();
    }
    private void PickNext()
    {
        if (state == BossState.Dead || state == BossState.Stunned) return;

        if (pendingPattern != null)
        {
            BossPattern next = pendingPattern;
            pendingPattern = null;
            Debug.Log($"[Boss] 예약 패턴 실행: {next.name} ({phaseIndex + 1}페이즈)");
            StartPattern(next);                          // [변경]
            return;
        }
        List<BossPattern> candidates = phases[phaseIndex].patterns.FindAll(p=> p != null && p.CanUse() && p.InRange(transform.position, ctx.target.position));

        if(candidates.Count == 0) 
        {
            state = BossState.Chase; // 쿨타임 중에는 추적
            ResumeMoving();
            waitRoutine = StartCoroutine(WaitThenPick(retryDelay));
            return;
        }
        BossPattern picked = candidates[Random.Range(0, candidates.Count)];

        Debug.Log($"[Boss] 패턴 실행: {picked.name} ({phaseIndex + 1}페이즈)");

        StartPattern(picked);   
    }

    void StartPattern(BossPattern pattern)
    {
        StopMoving();
        current = pattern;
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
        if(pendingStunDuration > 0f)
        {
            float d = pendingStunDuration, m = pendingStunMultiplier;
            pendingStunDuration = 0f;
            pendingStunMultiplier = 1f;
            Stun(d,m);
            return;
        }
        state = BossState.Chase;   // 쉬는 동안 추적
        ResumeMoving();
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
        bool interrupt = false;

        int next = phaseIndex + 1;
        if (next < phases.Count && ratio <= phases[next].hpRatio)
        {
            phaseIndex = next;
            if (phases[phaseIndex].entryPhasePattern != null)
                pendingPattern = phases[phaseIndex].entryPhasePattern;
            Debug.Log($"[Boss] {phaseIndex + 1}페이즈 진입 (HP {ratio:P0})");
            interrupt = true;
        }
        // 기믹이 진행 중일 때는 검사하지 않음 → 다음 피격 때 다시 확인
        if (gimmickPattern != null && !gimmickPattern.IsRunning)
        {
            float[] triggers = gimmickPattern.HpTriggers;
            bool fired = false;
            while (shieldTriggerIndex < triggers.Length && ratio <= triggers[shieldTriggerIndex])
            {
                shieldTriggerIndex++; // 한 번에 여러 개 넘어도 보호막은 1번만
                fired = true;
            }
            if (fired)
            {
                gimmickPattern.TriggerCount = shieldTriggerIndex;
                pendingPattern = gimmickPattern;
                Debug.Log($"[Boss] 보호막 기믹 트리거 {shieldTriggerIndex} (HP {ratio:P0})");
                interrupt = true;
            }
        }
        if (!interrupt) return;
        if (state == BossState.Stunned) return; // 기절 끝나면 pendingPattern부터 실행됨
        InterruptCurrent();
        state = BossState.Idle;
        PickNext();
    }
    void HandleDamaged(DamageInfo info)
    {
        OnHpChanged(health.Current / health.Max); 
        //Debug.Log(health.Current / health.Max);
    }
    Vector3 FlatDirTo(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;
        dir.y = 0f;
        return dir;
    }
    void UpdateChase()
    {
        if (ctx.target == null) return;
        Vector3 dir = FlatDirTo(ctx.target.position);
        movement.LookAt(dir);
        movement.MoveTo(ctx.target.position);
    }
    void StopMoving()
    {
        if (agent == null || !agent.isOnNavMesh) return;
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }
    void ResumeMoving()
    {
        if (agent == null || !agent.isOnNavMesh) return;
        agent.isStopped = false;
    }
    public void Stun(float duration, float damageMultiplier = 1f) // 보호막 파괴 성공 시 사용
    {
        if (state == BossState.Dead) return;
        InterruptCurrent();
        if (stunRoutine != null) StopCoroutine(stunRoutine);
        health.SetDamageMultiplier(damageMultiplier);
        stunRoutine = StartCoroutine(StunRoutine(duration));
    }
    IEnumerator StunRoutine(float duration)
    {
        state = BossState.Stunned;
        yield return new WaitForSeconds(duration);
        stunRoutine = null;
        health.SetDamageMultiplier(1f);
        state = BossState.Idle;
        PickNext();
    }

    void HandleDied()
    {
        InterruptCurrent();
        if (stunRoutine != null) StopCoroutine(stunRoutine);
        health.SetDamageMultiplier(1f);
        state = BossState.Dead;
    }

    public override void OnHit(DamageInfo info) { } // 보스는 경직,넉백 없음

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visualRange);
    }
}
