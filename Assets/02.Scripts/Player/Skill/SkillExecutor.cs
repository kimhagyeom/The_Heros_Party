using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillExecutor : MonoBehaviour
{
    [SerializeField] private LayerMask enemyMask;

    private DPMTracker dpmTracker;

    void Awake()
    {
        dpmTracker = GetComponent<DPMTracker>();
    }

    public void Execute(PlayerController player, SkillData data)
    {
        if (data.atk_Type == AttackType.Melee && data.hitbox_Shape == HitboxShape.Sector)
        {
            StartCoroutine(MeleeSectorRoutine(player, data));
            return;
        }

        Debug.Log($"{data.skill_Name}: {data.atk_Type}/{data.hitbox_Shape} 스킬은 아직 구현 안 됨");
    }

    IEnumerator MeleeSectorRoutine(PlayerController player, SkillData data)
    {
        HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();
        float elapsed = 0f;

        do
        {
            foreach (IDamageable target in HitDetector.CheckSector(player.transform, data.range, data.angle, enemyMask))
            {
                if (!hitTargets.Add(target)) continue;
                DealDamage(player, target, data);
            }

            elapsed += Time.deltaTime;
            yield return null;
        } while (elapsed < data.active_Time);
    }

    void DealDamage(PlayerController player, IDamageable target, SkillData d)
    {
        float damage = Mathf.Floor(player.stats.base_Atk * d.dmg_Rate + 0.5f);

        Vector3 hitDirection = target.transform.position - player.transform.position;
        hitDirection.y = 0f;

        DamageInfo damageInfo = new DamageInfo
        {
            damage = damage,
            infection = 0f,
            hitDirection = hitDirection.normalized,
            knockbackDistance = 0f
        };
        target.TakeDamage(damageInfo);

        if (dpmTracker != null)
            dpmTracker.RecordDamage(damage);
    }
}
