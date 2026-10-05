using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Enemy : MonoBehaviour
{
    private Health health;
    private EnemyMovement movement;
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private DamageText damageTextPrefab;
    [SerializeField] private Vector3 damageTextOffset = new Vector3(0, 2f, 0);

    public EnemyData Data => enemyData;
    public bool IsDead => health.IsDead;

    void Awake()
    {
        health = GetComponent<Health>();
        movement = GetComponent<EnemyMovement>();
        health.OnDamaged += ShowDamageText;
        health.OnDamaged += movement.OnHit;
        health.OnDied += Die;
    }

    public virtual void Start()
    {
        health.Init(enemyData.max_HP);

        EnemyRegistry.Instance.Register(this);
    }

    void ShowDamageText(DamageInfo damageInfo)
    {
        if (damageTextPrefab == null) return;

        DamageText dt = Instantiate(damageTextPrefab, transform.position + damageTextOffset, Quaternion.identity);
        dt.Setup(damageInfo.damage);
    }
    void Die()
    {
        EnemyRegistry.Instance.Unregister(this);
        Destroy(gameObject);
    }
}
