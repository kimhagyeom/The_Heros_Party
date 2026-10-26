
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossPattern : MonoBehaviour
{
    [SerializeField] protected BossPatternData data;

    public bool IsRunning { get; private set; }

    float lastEndTime = -999f;           // 마지막으로 끝난 시각 (쿨타임용)
    Coroutine routine;                    // 멈출 때 필요
    Action onEnd;                         // 끝나면 컨트롤러에 알릴 함수
    readonly List<GameObject> tracked = new();     // 정리할 오브젝트 목록

    public virtual bool CanUse()
    {
        if (IsRunning || data == null) return false;
        return Time.time >= lastEndTime + data.cooldown;
    }   
    public void Run(BossContext ctx ,Action onEnd) // 시작
    {
        if(IsRunning) return;
        this.onEnd = onEnd;
        IsRunning = true;
        routine = StartCoroutine(Wrapper(ctx));
    }    
    public void Stop()
    {
        if (!IsRunning) return;

        if (routine != null) StopCoroutine(routine);
        routine = null;

        foreach (var obj in tracked)
            if (obj != null) Destroy(obj);   // 이펙트가 이미 사라졌을 수 있으니 null 체크
        tracked.Clear();

        OnStopped();
        IsRunning = false;
        onEnd = null;                         // 중지 시에는 알리지 않음
    }            

    protected abstract IEnumerator Execute(BossContext ctx);   // 패턴마다 구현 자식에서 실행
    
    protected virtual void OnStopped() { }      // 패턴별 추가 정리 (선택)
    protected GameObject Track(GameObject obj)  // 생성한 것 등록
    {
        tracked.Add(obj);
        return obj;
    } 

    IEnumerator Wrapper(BossContext ctx)
    {
        yield return Execute(ctx);

        routine = null;
        tracked.Clear();          // 정상 종료: 지우지 않고 목록만 비움
        IsRunning = false;
        lastEndTime = Time.time;  // 쿨타임은 끝난 시각 기준

        var callback = onEnd;
        onEnd = null;
        callback?.Invoke();
    }    
}