using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillExecutor : MonoBehaviour
{
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private LayerMask obstacleMask = 1;    // 순간이동을 막는 벽 레이어 (기본: Default)
    [SerializeField] private float behindDistance = 1f;     //일섬?: 대상 중심에서 얼마나 뒤로 이동할지

    private DPMTracker dpmTracker;
    private CharacterController characterController;

    void Awake()
    {
        dpmTracker = GetComponent<DPMTracker>();
        characterController = GetComponent<CharacterController>();
    }

    //일섬?:플레이어와 range 안에 있는 적 중 커서에 가장 가까운 적, 없으면 null
    public Enemy FindTarget(PlayerController player, SkillData data)
    {
        if (!player.TryGetMouseWorldPosition(out Vector3 mousePos)) return null;

        float sqrRange = data.range * data.range;
        Enemy best = null;
        float bestSqrToMouse = float.MaxValue;

        foreach (Enemy enemy in EnemyRegistry.Instance.ActiveEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;

            Vector3 toEnemy = enemy.transform.position - player.transform.position;
            toEnemy.y = 0f;
            if (toEnemy.sqrMagnitude > sqrRange) continue;

            Vector3 toMouse = enemy.transform.position - mousePos;
            toMouse.y = 0f;
            if (toMouse.sqrMagnitude < bestSqrToMouse)
            {
                bestSqrToMouse = toMouse.sqrMagnitude;
                best = enemy;
            }
        }
        return best;
    }

    // 일섬 경로 길이
    public float GetPathLength(Transform owner, Enemy target, SkillData data)
    {
        Vector3 toTarget = target.transform.position - owner.position;
        toTarget.y = 0f;
        return Mathf.Min(toTarget.magnitude, data.length);
    }

    public void Execute(PlayerController player, SkillData data, Enemy target)
    {
        if (data.atk_Type == AttackType.Melee && data.hitbox_Shape == HitboxShape.Sector)
        {
            StartCoroutine(MeleeSectorRoutine(player, data));
            return;
        }

        if (data.move_Type == MoveType.Target_Back && target != null)
        {
            TargetSlash(player, data, target);
            return;
        }

        Debug.Log($"{data.skill_Name}: {data.atk_Type}/{data.hitbox_Shape} 스킬은 아직 구현 안 됨");
    }

    // 일섬?: 대상까지의 경로에 있는 적과 대상을 한 번에 베고 대상의 뒤로 순간이동
    void TargetSlash(PlayerController player, SkillData data, Enemy target)
    {
        Transform owner = player.transform;
        Vector3 toTarget = target.transform.position - owner.position;
        toTarget.y = 0f;
        Vector3 dir = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : owner.forward;
        player.FaceDirection(dir);

        float pathLength = Mathf.Max(GetPathLength(owner, target, data), 0.1f);
        float moveDistance = GetDistanceBeforeWall(owner, dir, toTarget.magnitude + behindDistance);
        Vector3 destination = owner.position + dir * moveDistance;

        HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();
        foreach (IDamageable hit in HitDetector.CheckBox(owner, data.width, pathLength, enemyMask))
            hitTargets.Add(hit);
        if (target.TryGetComponent(out IDamageable targetDamageable))
            hitTargets.Add(targetDamageable); // 대상은 경로(최대 length) 밖이어도 항상 맞음

        foreach (IDamageable hit in hitTargets)
            DealDamage(player, hit, data);

        player.Teleport(destination);
    }

    // 캐릭터 몸통 크기의 구를 이동 방향으로 쏴서 벽에 닿으면 그 앞까지만
    float GetDistanceBeforeWall(Transform owner, Vector3 dir, float distance)
    {
        if (characterController == null) return distance;

        Vector3 center = owner.position + characterController.center;
        if (Physics.SphereCast(center, characterController.radius, dir, out RaycastHit hit,
                distance, obstacleMask, QueryTriggerInteraction.Ignore))
            return hit.distance;

        return distance;
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
