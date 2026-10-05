using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private LayerMask targetMask;
    private MeleeAttack meleeAttack = new MeleeAttack();
    private ProjectileAttack projectileAttack = new ProjectileAttack();
    private AoeAttack aoeAttack = new AoeAttack();

    void Start()
    {
    }
    public void Attack(EnemyAtkData d, Vector3 targetPos)
    {
        switch (d.atk_Type)
        {
            case AttackType.Melee:
                meleeAttack.CheckAttackType(transform, d, targetMask);
                break;
            case AttackType.Aoe:
                aoeAttack.Execute(targetPos, d, targetMask);
                SpawnEffect(d, targetPos);
                break;
            case AttackType.Projectile:
                projectileAttack.ShootProjectile(transform, d, targetMask);
                break;
        }
    }
    public void SetTargetMask(LayerMask mask)
    {
        targetMask = mask;
    }
    public void SpawnEffect(EnemyAtkData d, Vector3 targetPos)
    {
        if (d.effectPrefab == null)
            return;

        Vector3 spawnPos = targetPos + Vector3.up * 0.02f;

        GameObject effect = Instantiate(d.effectPrefab, spawnPos, transform.rotation);
    }
}
