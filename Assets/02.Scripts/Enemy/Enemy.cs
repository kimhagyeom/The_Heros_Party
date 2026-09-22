using UnityEngine;

public class Enemy : MonoBehaviour,IDamageable
{
    private int maxHealth;
    private int currentHealth;
    [SerializeField] private EnemyData enemyData;
    public virtual void Start()
    {
        maxHealth = enemyData.maxHP;
        currentHealth = maxHealth;

        EnemyRegistry.Instance.Register(this);
    }

    
    public void TakeDamage(float amount)
    {
        currentHealth -= (int)amount;
        if(currentHealth <= 0)
        {
            Die();
        }
        Debug.Log($"Enemy took {amount} damage!");
    }
    void Die()
    {
        EnemyRegistry.Instance.Unregister(this);
        Destroy(gameObject);
    }
}
