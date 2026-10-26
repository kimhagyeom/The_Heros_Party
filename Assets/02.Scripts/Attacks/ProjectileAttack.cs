using UnityEngine;

public class ProjectileAttack
{
    // EnemyAtkData가 있는 일반 몬스터
    public void ShootProjectile(Transform origin, EnemyAtkData d, LayerMask targetLayer)
    {
        if (d.projectilePrefab == null)
        {
            Debug.LogWarning($"{d.atk_Name}: projectilePrefab이 비어 있음");
            return;
        }

        Vector3 spawnPos = origin.position + origin.forward * 0.5f;

        ProjectileSettings settings = new ProjectileSettings
        {
            speed = d.active_Time > 0f ? d.atk_Range / d.active_Time : 0f, // 속도 시트 추가 문의
            lifeTime = d.active_Time,
            radius = d.radius,
            pierce = true,            // 적 투사체는 맞아도 사라지지 않음
            targetMask = targetLayer,
            obstacleMask = 0          // 적 투사체는 벽 무시
        };
        DamageInfo dmg = new DamageInfo
        {
            damage = d.hp_Damage,
            infection = d.infection_Value,
            knockbackDistance = d.has_Knockback ? d.knockback_Distance : 0f
        };

        Shoot(d.projectilePrefab, spawnPos, origin.rotation, settings, dmg);
       
    }

    // 단순 쏘기
    public Projectile Shoot(Projectile prefab, Vector3 pos, Quaternion rotation, ProjectileSettings settings, DamageInfo damageInfo)
    {
        Projectile p = Object.Instantiate(prefab, pos, rotation);
        p.Init(settings, target =>
        {
            DamageInfo dmg = damageInfo;
            dmg.hitDirection = (target.transform.position - p.transform.position).normalized;
            target.TakeDamage(dmg);
        });
        return p;
    }
}
