using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Enemy : MonoBehaviour
{
    private Health health;
    private EnemyAI_S ai;
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
        ai = GetComponent<EnemyAI_S>();
        health.OnDamaged += ShowDamageText;
        health.OnDamaged += OnHit;
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
    public void OnHit(DamageInfo info)
    {
        if (ai != null && ai.IsSuperArmor) return;

        // 경직과 넉백이 동시에 발생한다면 
        // if(!enemyData.can_Knockback && !enemyData.can_Stagger) return;
        //ai.Stagger(enemyData.stagger_Duration);
        //movement.KnockBack(info);
        if(enemyData.can_Stagger)
            ai.Stagger(enemyData.stagger_Duration);
        if(enemyData.can_Knockback)
            movement.KnockBack(info);
    }
    void Die()
    {
        EnemyRegistry.Instance.Unregister(this);
        Destroy(gameObject);
    }
}
