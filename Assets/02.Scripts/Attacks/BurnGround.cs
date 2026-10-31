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
    private AoeAttack aoeAttack = new AoeAttack();
    private BurnGroundSettings settings;
    private DamageInfo dmgInfo;
    private float elapsed;
    private float stayTime = 0f;

    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= settings.lifeTime) { Destroy(gameObject); return; }
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
        transform.localScale = new Vector3(settings.radius, 0.2f, settings.radius);
    }
}
