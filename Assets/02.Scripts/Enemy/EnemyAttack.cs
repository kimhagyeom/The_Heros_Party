using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private LayerMask targetMask;
    private MeleeAttack meleeAttack;
    private ProjectileAttack projectileAttack;
    private AoeAttack aoeAttack;

    void Start()
    {
        meleeAttack = new MeleeAttack();
        projectileAttack = new ProjectileAttack();
        aoeAttack = new AoeAttack();
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
