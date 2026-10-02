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
        p.Init(d, targetLayer);
    }
}
