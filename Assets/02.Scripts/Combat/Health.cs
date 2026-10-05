using System;
using UnityEngine;

// 플레이어/적 공용 체력
// 피격 후 처리(데미지 텍스트, 사망 연출 등)는 각 주인이 이벤트를 구독해서 처리
public class Health : MonoBehaviour, IDamageable
{
    private float maxHealth;
    private float currentHealth;
    private bool isDead = false;
    private float invincibleUntil = 0f;

    public float Max => maxHealth;
    public float Current => currentHealth;
    public bool IsDead => isDead;
    public bool IsInvincible => Time.time < invincibleUntil;

    public event Action<DamageInfo> OnDamaged; // 받은 데미지량
    public event Action OnDied;

    public void Init(float max, float start)
    {
        maxHealth = max;
        currentHealth = Mathf.Min(start, max);
        isDead = false;
        invincibleUntil = 0f;
    }

    public void Init(float max)
    {
        Init(max, max);
    }

    // 무적이 겹치면 더 늦게 끝나는 쪽을 유지 (대쉬 무적, 피격 무적이 서로 끄지 않도록)
    public void AddInvincible(float duration)
    {
        invincibleUntil = Mathf.Max(invincibleUntil, Time.time + duration);
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (IsInvincible) Debug.Log("무적 , 데미지무시");
        if (isDead || IsInvincible) return;

        currentHealth -= damageInfo.damage;
        OnDamaged?.Invoke(damageInfo);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
            OnDied?.Invoke();
        }
    }
}
