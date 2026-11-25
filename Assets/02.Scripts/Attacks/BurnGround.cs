using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using System;


public struct BurnGroundSettings
{
    public float radius;
    public float lifeTime;
    public float damage;
    public LayerMask targetMask;
}
public class BurnGround : MonoBehaviour
{
    [SerializeField] private float tickInterval;
    
    //private Dictionary<IDamageable, float> stayTimer = new Dictionary<IDamageable, float>();

    [Header("Visual")]
    [SerializeField] private ParticleSystem fireFx;   // 불꽃 파티클 (루트)
    [SerializeField] private Transform areaVisual;
    private BurnGroundSettings settings;
    private float elapsed;
    private float stayTime = 0f;
    private bool isEnding;


    void Update()
    {
        if(isEnding) return;
        elapsed += Time.deltaTime;
        if (elapsed >= settings.lifeTime) { 
            EndBurn();
            return; 
        }
        
        List<IDamageable> hits = HitDetector.CheckSectorAoe(transform.position, settings.radius, 360f, settings.targetMask);

        if(hits.Count > 0)
        {
            stayTime += Time.deltaTime;
            if(stayTime >= tickInterval)
            {
                stayTime -= tickInterval;
                hits[0].TakeDamage(new DamageInfo{ damage = settings.damage});
            }
        }
        else stayTime = 0f;
    }

    public void Init(BurnGroundSettings s)
    {
        settings = s;
        // 범위 메쉬만 스케일 (기본 Cylinder/Quad는 지름 1 기준이라 * 2)
        float diameter = settings.radius * 2f;
        areaVisual.localScale = new Vector3(diameter, areaVisual.localScale.y, diameter);

        // 프리팹에서 Play On Awake를 꺼두고, 여기서 직접 재생
        fireFx.Play(true);
    }
    private void EndBurn()
    {
        isEnding = true;   // 판정 종료 (다시 들어오지 않게)

        if (areaVisual != null)
            areaVisual.gameObject.SetActive(false);

        // 새로 뿜는 것만 멈추고, 떠 있는 불꽃은 자연스럽게 사라지게 둠
        fireFx.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        // 남은 불꽃이 다 사라질 시간만큼 기다렸다가 파괴
        Destroy(gameObject, GetMaxParticleLifetime());
    }

    // 자식 파티클까지 포함해서 가장 긴 Start Lifetime 구하기
    private float GetMaxParticleLifetime()
    {
        float max = 0f;
        foreach (var ps in fireFx.GetComponentsInChildren<ParticleSystem>())
        {
            float life = ps.main.startLifetime.constantMax;
            if (life > max) max = life;
        }
        return max;
    }
}
