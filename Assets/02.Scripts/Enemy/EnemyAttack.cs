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
            case Enemy_Atk_Type.Melee:
                meleeAttack.CheckAttackType(transform, d, targetMask);
                break;
            case Enemy_Atk_Type.Projectile:
                break;
        }
    }
    public void SetTargetMask(LayerMask mask)
    {
        targetMask = mask;
    }
}
