using UnityEngine;
using System.Collections.Generic;

public class AoeAttack
{
    public void Execute(Vector3 targetPos, EnemyAtkData d, LayerMask targetLayer)
    {
        List<IDamageable> targets = new List<IDamageable>();

        switch (d.hitbox_Shape)
        {
            case HitboxShape.Box : 
                targets = HitDetector.CheckBoxAoe(targetPos,d.width, d.length, targetLayer);
                break;
            case HitboxShape.Sector : 
                targets = HitDetector.CheckSectorAoe(targetPos, d.radius, d.angle, targetLayer);
                break;   
        }

        foreach (IDamageable target in targets)
        {
            target.TakeDamage(d.hp_Damage);
        }
    }
}
