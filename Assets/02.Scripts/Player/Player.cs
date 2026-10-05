using UnityEngine;

// 플레이어 본체: Health 알림을 받아서 반응을 결정하고 지시
// (Enemy와 같은 역할. 입력·이동·대시·넉백 실행은 PlayerController가 담당)
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerController))]
public class Player : MonoBehaviour
{
    [SerializeField] private DamageText damageTextPrefab;
    [SerializeField] private Vector3 damageTextOffset = new Vector3(0, 2f, 0);

    private Health health;
    private PlayerController controller;

    private PlayerStats Stats => controller.stats;
    public bool IsDead => health.IsDead;

    void Awake()
    {
        health = GetComponent<Health>();
        controller = GetComponent<PlayerController>();

        health.Init(Stats.maxHealth, Stats.startHealth);
    }

    void OnEnable()
    {
        health.OnDamaged += OnHit;
        health.OnDied += Die;
    }

    void OnDisable()
    {
        health.OnDamaged -= OnHit;
        health.OnDied -= Die;
    }

    // 실제 HP 감소는 Health가 처리하고, 여기선 피격 후 반응만
    void OnHit(DamageInfo info)
    {
        ShowDamageText(info.damage);
        health.AddInvincible(Stats.hit_Invincible_Time);

        // 넉백: 플레이어는 기본적으로 안 밀리고, 공격 쪽이 넉백 거리를 보냈을 때만 밀림
        if (info.knockbackDistance > 0f)
            controller.Knockback(info.hitDirection, info.knockbackDistance);

        // 감염도: 공격에 감염도가 있으면 증가, 최대치 도달 시 사망
        if (info.infection > 0f)
        {
            Stats.currentInfection = Mathf.Clamp(Stats.currentInfection + info.infection, 0f, Stats.max_Infection);
            if (Stats.currentInfection >= Stats.max_Infection)
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
        Debug.Log("Player has died!");
        GameManager.Instance.GameOver();
    }
}