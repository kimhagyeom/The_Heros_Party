
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
        if (IsRunning) return false;
        return Time.time >= lastEndTime + data.cooldown;
    }   
    public void Run(Action onEnd) // 시작
    {
        if(IsRunning) return;
        this.onEnd = onEnd;
        IsRunning = true;
        routine = StartCoroutine(Wrapper());
    }    
    public void Stop()  // 페이즈 페턴 진입 같은 경우에 강제 종료
    {
        
    }              

    protected abstract IEnumerator Execute();   // 패턴마다 구현 자식에서 실행
    
    protected virtual void OnStopped() { }      // 패턴별 추가 정리 (선택)
    protected GameObject Track(GameObject obj)  // 생성한 것 등록
    {
        tracked.Add(obj);
        return obj;
    } 

    IEnumerator Wrapper()
    {
        yield return null; //StartCoroutine(Execute());
    }           
}