using UnityEngine;
using System.Collections.Generic;

public class AoeAttack
{
    public void Execute(Vector3 pos, HitboxInfo hitbox , DamageInfo damageInfo, LayerMask targetLayer)
    {
        List<IDamageable> targets = hitbox.shape switch
        {
             HitboxShape.Box    => HitDetector.CheckBoxAoe(pos, hitbox.width, hitbox.length, targetLayer),
            HitboxShape.Sector => HitDetector.CheckSectorAoe(pos, hitbox.radius, hitbox.angle, targetLayer),
            _=> new List<IDamageable>()
        };

        foreach(IDamageable target in targets)
        {
            DamageInfo dmg = damageInfo;
            dmg.hitDirection = (target.transform.position - pos).normalized;
            target.TakeDamage(dmg);
        }
    }


    // 일반 적용: 변환 후 핵심 호출 (적 쪽 코드는 그대로)
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
            DamageInfo damageInfo = new DamageInfo
            {
                damage = d.hp_Damage,
                infection = d.infection_Value,
                hitDirection = (target.transform.position - targetPos).normalized,
                knockbackDistance = d.has_Knockback ? d.knockback_Distance : 0f
            };
            target.TakeDamage(damageInfo);
        }
    }

}
