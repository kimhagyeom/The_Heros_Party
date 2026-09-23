using UnityEngine;

public class Enemy : MonoBehaviour,IDamageable
{
    private int maxHealth;
    private int currentHealth;
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private DamageText damageTextPrefab;                     
    [SerializeField] private Vector3 damageTextOffset = new Vector3(0, 2f, 0); //머리 위 높이

    public virtual void Start()
    {
        maxHealth = enemyData.maxHP;
        currentHealth = maxHealth;

        EnemyRegistry.Instance.Register(this);
    }

    
    public void TakeDamage(float amount)
    {
        currentHealth -= (int)amount;
        ShowDamageText(amount); //죽기 전에 띄워야 함
        if(currentHealth <= 0)
        {
            Die();
        }
        Debug.Log($"Enemy took {amount} damage!");
    }
    void ShowDamageText(float amount)
    {
        if (damageTextPrefab == null) return;

        DamageText dt = Instantiate(damageTextPrefab, transform.position + damageTextOffset, Quaternion.identity);
        dt.Setup(amount);
    }
    void Die()
    {
        EnemyRegistry.Instance.Unregister(this);
        Destroy(gameObject);
    }
}
