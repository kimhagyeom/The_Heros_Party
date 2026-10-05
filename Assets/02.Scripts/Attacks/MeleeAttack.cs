using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack
{
    public void CheckAttackType(Transform owner, EnemyAtkData d, LayerMask targetMask)
    {
        List<IDamageable> targets = new List<IDamageable>();

        switch (d.hitbox_Shape)
        {
            case HitboxShape.Box : 
                targets = HitDetector.CheckBox(owner,d.width, d.length, targetMask);
                break;
            case HitboxShape.Sector : 
                targets = HitDetector.CheckSector(owner, d.radius, d.angle, targetMask);
                break;   
        }
        foreach(IDamageable target in targets)
        {
            DamageInfo damageInfo = new DamageInfo
            {
                damage = d.hp_Damage,
                infection = d.infection_Value,
                hitDirection = (target.transform.position - owner.position).normalized,
                knockbackDistance = d.has_Knockback ? d.knockback_Distance : 0f
            };
            target.TakeDamage(damageInfo);
        }
        
    }
}
