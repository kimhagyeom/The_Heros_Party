using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float hitRadius;
    private float damage = 10f;
    [SerializeField] private float lifeTime = 15f;
    private LayerMask targetLayer;
    private EnemyAtkData atkData;
    private float speed;
    private HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
    void Update()
    {
        Flying();
 
        CheckHit();
 
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
        {
            Debug.Log("Destroy");
            Destroy(gameObject);
        }
            
    }
 
    public void Init(EnemyAtkData d, LayerMask mask)
    {
        atkData = d;
        targetLayer = mask;
        damage = d.hp_Damage;
        hitRadius = d.radius;
        speed = d.atk_Range / d.active_Time;  
        
        if (d.radius > 0f) hitRadius = d.radius;
        if (d.active_Time > 0f) lifeTime = d.active_Time;
    }
    void Flying()
    {
        transform.position += transform.forward * speed * Time.deltaTime; // 속도 시트 추가 문의 
    }
    void CheckHit()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius, targetLayer);
        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out IDamageable target)) continue;
            if (!hitTargets.Add(target)) continue;   // 이미 맞은 대상이면 건너뜀

            target.TakeDamage(damage);
        }
    }
}
