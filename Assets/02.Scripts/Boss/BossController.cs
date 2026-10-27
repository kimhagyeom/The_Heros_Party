using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.PlayerLoop;

[Serializable]
public class PhaseInfo
{
    public float hpRatio;              // 이 페이즈가 시작되는 체력 비율
    public List<BossPattern> patterns; // 이 페이즈에서 쓰는 패턴
}
public class BossController : MonoBehaviour
{
    [SerializeField] List<PhaseInfo> phases;
    [SerializeField] float idleTime;
    [SerializeField] float retryDelay = 0.2f;
    [SerializeField] private AttackTelegraph telegraph;

    BossContext ctx;
    BossPattern current; 
    int phaseIndex;

    bool started; //보스 방 입장

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
        
        PickNext();
    }
    private void PickNext()
    {
        if(current == null) current = phases[0].patterns[0];
        current.Run(ctx, OnPatternEnd);
    }
    private void OnPatternEnd() // current 비우고 PickNext
    {
        if(current != null) current = null;
    }
}
