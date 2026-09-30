using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float hitRadius = 0.5f;
    private float damage = 10f;
    private float lifeTime = 15f;
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
    void Update()
    {
        Flying();
 
        IDamageable target = Hit();
        if (target != null)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
 
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
            Destroy(gameObject);
    }
    void Flying()
    {
        transform.position += transform.forward * 10f * Time.deltaTime;
    }
    IDamageable Hit()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius,  LayerMask.GetMask("Player"));
        if (hits.Length == 0) return null;
 
        if (hits[0].TryGetComponent(out IDamageable damageable))
            return damageable;
 
        return null;
    }
}
