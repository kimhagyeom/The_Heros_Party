using UnityEngine;

public class ProjectileAttack
{
    public void ShootProjectile(Transform origin, EnemyAtkData d, LayerMask targetLayer)
    {
        if (d.projectilePrefab == null)
        {
            Debug.LogWarning($"{d.atk_Name}: projectilePrefab이 비어 있음");
            return;
        }

        Vector3 spawnPos = origin.position + origin.forward * 0.5f;

        Projectile p = Object.Instantiate(d.projectilePrefab, spawnPos, origin.rotation);
        ProjectileSettings settings = new ProjectileSettings
        {
            speed = d.active_Time > 0f ? d.atk_Range / d.active_Time : 0f, // 속도 시트 추가 문의
            lifeTime = d.active_Time,
            radius = d.radius,
            pierce = true,            // 적 투사체는 맞아도 사라지지 않음
            targetMask = targetLayer,
            obstacleMask = 0          // 적 투사체는 벽 무시
        };
        p.Init(settings, target => target.TakeDamage(new DamageInfo
        {
            damage = d.hp_Damage,
            infection = d.infection_Value,
            hitDirection = (target.transform.position - p.transform.position).normalized,
            knockbackDistance = d.has_Knockback ? d.knockback_Distance : 0f
        }));
    }
}
