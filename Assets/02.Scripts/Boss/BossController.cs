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
public class BossController : Enemy
{    
    [SerializeField] List<PhaseInfo> phases;
    [SerializeField] float idleTime;
    [SerializeField] float retryDelay = 0.2f;
    [SerializeField] private AttackTelegraph telegraph;

    BossContext ctx;
    BossPattern current; 
    BossPattern pendingPattern;
    int phaseIndex;

    bool started; //보스 방 입장

    public override void Start()
    {
        base.Start();
        health.OnDamaged += HandleDamaged;
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
            telegraph = telegraph
        };
        phaseIndex = 0;
        PickNext();
    }
    private void PickNext()
    {
        if(pendingPattern != null)
        {
            current = pendingPattern;
            pendingPattern = null;
            current.Run(ctx, OnPatternEnd);
            return;
        }
        List<BossPattern> candidates = phases[phaseIndex].patterns.FindAll(p=> p != null && p.CanUse());
        if(candidates.Count == 0) 
        {
            StartCoroutine(WaitThenPick(retryDelay));
            return;
        }
        current = candidates[Random.Range(0, candidates.Count)];
        current.Run(ctx, OnPatternEnd);
    }
    private IEnumerator WaitThenPick(float delay)
    {
        yield return new WaitForSeconds(delay);
        PickNext();
    }
    private void OnPatternEnd() // current 비우고 PickNext
    {
        current = null;

        StartCoroutine(WaitThenPick(idleTime));
    }

    public void OnHpChanged(float ratio) // 추후 Hp 처리에서 사용 예정
    {
        int next = phaseIndex + 1;
        if (next < phases.Count && ratio <= phases[next].hpRatio)
        {
            phaseIndex = next;
            pendingPattern = phases[phaseIndex].entryPhasePattern;
        }
    }
    void HandleDamaged(DamageInfo info)
    {
        OnHpChanged(health.Current / health.Max); 
        Debug.Log(health.Current / health.Max);
    }
}
