using UnityEngine;

public class Enemy : MonoBehaviour,IDamageable
{
    private int maxHealth;
    private int currentHealth;
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private DamageText damageTextPrefab;                     
    [SerializeField] private Vector3 damageTextOffset = new Vector3(0, 2f, 0); //�Ӹ� �� ����

    public virtual void Start()
    {
        maxHealth = enemyData.maxHP;
        currentHealth = maxHealth;

        EnemyRegistry.Instance.Register(this);
    }

    
    public void TakeDamage(float amount)
    {
        currentHealth -= (int)amount;
        ShowDamageText(amount); //�ױ� ���� ����� ��
        if(currentHealth <= 0)
        {
            Die();
        }
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
