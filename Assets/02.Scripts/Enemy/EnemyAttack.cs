using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private LayerMask targetMask;
    private MeleeAttack meleeAttack;
    private ProjectileAttack projectileAttack;

    void Start()
    {
        meleeAttack = new MeleeAttack();
        projectileAttack = new ProjectileAttack();
    }
    public void Attack(EnemyAtkData d)
    {
        switch (d.atk_Type)
        {
            case AttackType.Melee:
                meleeAttack.CheckAttackType(transform, d, targetMask);
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
}
