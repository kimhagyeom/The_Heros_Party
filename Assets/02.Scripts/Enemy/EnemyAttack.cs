using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private LayerMask targetMask;
    private MeleeAttack meleeAttack;
    public Projectile projectilePrefab;

    void Start()
    {
        meleeAttack = new MeleeAttack();
    }
    public void Attack(EnemyAtkData d)
    {
        switch (d.atk_Type)
        {
            case AttackType.Melee:
                meleeAttack.CheckAttackType(transform, d, targetMask);
                break;
            case AttackType.Projectile:
                break;
        }
    }
    public void SetTargetMask(LayerMask mask)
    {
        targetMask = mask;
    }
}
