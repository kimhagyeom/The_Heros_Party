using System;
using Unity.VisualScripting;
using UnityEngine;

// 플레이어/적 공용 체력
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
    [Header("실드")]
    public float shieldHP;
    private float damageMultiplier = 1f;
    public bool IsShielded => shieldHP > 0;
    public void SetDamageMultiplier(float m) => damageMultiplier = m;
    //Action

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


        damageInfo.damage *= damageMultiplier;
        float damage = damageInfo.damage;

        // 보호막이 있으면 보호막부터
        if (shieldHP > 0)
        {
            shieldHP -= damage;
            if (shieldHP > 0)
            {
                Debug.Log($"보호막 피격 -{damage} → 보호막 {shieldHP}");
                return;
            }                // 아직 안 깨짐 → 체력은 그대로

            // 깨짐: 넘친 피해만 체력으로 (변경: < 0 → <= 0, 정확히 0일 때도 깨짐 처리)
            float overflow = -shieldHP;
            shieldHP = 0f;
            Debug.Log($"보호막 깨짐! 넘친 피해 {overflow}");

            if (overflow <= 0f) return;               // 딱 맞게 깨졌으면 체력 피해 없음
            damage = overflow;
            damageInfo.damage = overflow;
        }

        // 체력 피해 (변경: 넘친 피해도 여기로 와서 OnDamaged가 불림)
        currentHealth -= damage;
        OnDamaged?.Invoke(damageInfo);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
            OnDied?.Invoke();
        }
    }
    public void SetShield(float amount)
    {
        shieldHP = amount;
    }
    public void DestroyShield()
    {
        shieldHP = 0f;
    }
}
