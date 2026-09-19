using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;

public class PlayerAttack : MonoBehaviour
{
    public float detectRange = 10f; // 감지 범위
    public float attackInterval = 1f; // 공격 간격
    private float lastAttackTime = -999f; // 마지막 공격 시간
    public float closeAttackRange = 8f; // 근거리 공격 범위
    public float closeAttackAngle = 65f; // 근거리 공격 각도

    public PlayerController playerController; // 플레이어 컨트롤러 참조
    public DPMTracker dpmTracker; // DPM 트래커 참조
    private List<Transform> enemiesInRange = new List<Transform>(); // 감지된 적 리스트

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        dpmTracker = GetComponent<DPMTracker>();
    }
    void Update()
{
    FindEnemy();
    if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
    {
        TryAttack();
    }
    if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            dpmTracker.ResetTracker();
        }
}

    void FindEnemy()
    {
        enemiesInRange.Clear(); 
        float sqrDetectRange = detectRange * detectRange;

        foreach (Enemy enemy in EnemyRegistry.Instance.ActiveEnemies)
        {
            float sqrDistance = (enemy.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance <= sqrDetectRange) 
            {
                enemiesInRange.Add(enemy.transform);
            }
        }
    }
    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackInterval)
        {
            return; // 아직 쿨타임
        }
        lastAttackTime = Time.time;
        Attack();
    }
    void Attack()
    {
        if (enemiesInRange.Count == 0)
        {
            Debug.Log("No enemies in range.");
            return;
        }

        float sqrAttackRange = closeAttackRange * closeAttackRange;
        bool hitAny = false;

        foreach (Transform enemy in enemiesInRange)
        {
            Vector3 dirToEnemy = enemy.position - transform.position;
            float sqrDist = dirToEnemy.sqrMagnitude;
            float angle = Vector3.Angle(transform.forward, dirToEnemy);

            if (sqrDist <= sqrAttackRange && angle <= closeAttackAngle)
            {
                if (enemy.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(playerController.atk);
                    dpmTracker.RecordDamage(playerController.atk); 
                    Debug.Log($"Attacked {enemy.name} for {playerController.atk} damage!");
                    hitAny = true;
                }
            }
        }

        if (!hitAny)
        {
            Debug.Log("No enemies in attack range or angle.");
        }
    }

    void OnDrawGizmos()
    {
        // Gizmos.color = Color.red;
        // Gizmos.DrawWireSphere(transform.position, detectRange);
        // Vector3 rightBoundary = Quaternion.Euler(0, attackAngle, 0) * transform.forward;
        // Vector3 leftBoundary = Quaternion.Euler(0, -attackAngle, 0) * transform.forward;
        // Gizmos.DrawLine(transform.position, transform.position + rightBoundary * detectRange);
        // Gizmos.DrawLine(transform.position, transform.position + leftBoundary * detectRange);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, closeAttackRange);
        Vector3 forwardBoundary = Quaternion.Euler(0, closeAttackAngle, 0) * transform.forward;
        Vector3 backwardBoundary = Quaternion.Euler(0, -closeAttackAngle, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + forwardBoundary * closeAttackRange);
        Gizmos.DrawLine(transform.position, transform.position + backwardBoundary * closeAttackRange);
    }
}
