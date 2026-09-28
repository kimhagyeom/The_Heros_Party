using UnityEngine;

public class Enemy : MonoBehaviour,IDamageable
{
    private float maxHealth;
    private float currentHealth;
    private bool isDead = false; //Die
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private DamageText damageTextPrefab;                     
    [SerializeField] private Vector3 damageTextOffset = new Vector3(0, 2f, 0);

    public virtual void Start()
    {
        maxHealth = enemyData.max_HP;
        currentHealth = maxHealth;

        EnemyRegistry.Instance.Register(this);
    }

    
    public void TakeDamage(float amount)
    {
        if (isDead) return; // Destroy는 프레임 끝에 처리되므로 같은 프레임의 추가 피격 무시
        currentHealth -= amount;
        ShowDamageText(amount);
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
        isDead = true;
        EnemyRegistry.Instance.Unregister(this);
        Destroy(gameObject);
    }
}
