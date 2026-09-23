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

    public Transform weaponPivot; // 빈 오브젝트, Player 자식으로 배치
    public float swingDuration = 0.25f;
    public float swingAngle = 190f;
    public float hitDetectionWidth = 15f; // 무기 판정 폭 (도 단위)
    private List<Transform> enemiesInRange = new List<Transform>(); // 감지된 적 리스트
 
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        dpmTracker = GetComponent<DPMTracker>();
    }
    void Update()
    {
        FindEnemy();
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
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
        //Attack();
        StartCoroutine(SwingAttack());
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
                    damageable.TakeDamage(playerController.stats.base_Attack);
                    dpmTracker.RecordDamage(playerController.stats.base_Attack); 
                    Debug.Log($"Attacked {enemy.name} for {playerController.stats.base_Attack} damage!");
                    hitAny = true;
                }
            }
        }

        if (!hitAny)
        {
            Debug.Log("No enemies in attack range or angle.");
        }
    }
    IEnumerator SwingAttack()
    {
        float facingSign = playerController.facingSign; // 1 or -1
        
        float startYaw = 0f;
        float endYaw = facingSign > 0 ? swingAngle : -swingAngle;

        HashSet<Transform> hit = new HashSet<Transform>();
        float elapsed = 0f;

        while (elapsed < swingDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / swingDuration;
            float currentYaw = Mathf.Lerp(startYaw, endYaw, t);

            // 무기 축을 실제로 회전 → 이펙트가 이 오브젝트를 따라가면 완벽히 싱크됨
            weaponPivot.rotation = Quaternion.Euler(0f, currentYaw, 0f);

            CheckWeaponHit(hit);

            yield return null;
        }
    }

    void CheckWeaponHit(HashSet<Transform> hit)
    {
        float sqrAttackRange = closeAttackRange * closeAttackRange;

        foreach (Transform enemy in enemiesInRange)
        {
            if (hit.Contains(enemy)) continue;

            Vector3 dirToEnemy = enemy.position - transform.position;
            if (dirToEnemy.sqrMagnitude > sqrAttackRange) continue;

            // "무기 축의 현재 방향"과 "적 방향" 사이 각도가 좁은 판정폭 안이면 맞은 걸로 처리
            float angleToWeapon = Vector3.Angle(weaponPivot.forward, dirToEnemy);
            if (angleToWeapon <= hitDetectionWidth) 
            {
                hit.Add(enemy);
                if (enemy.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(playerController.stats.base_Attack);
                    dpmTracker.RecordDamage(playerController.stats.base_Attack);
                }
            }
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

        if(weaponPivot != null)
        {
            Gizmos.color = Color.blue;
            Vector3 widthL = Quaternion.Euler(0, -hitDetectionWidth * 0.5f, 0) * weaponPivot.forward;
            Vector3 widthR = Quaternion.Euler(0, hitDetectionWidth * 0.5f, 0) * weaponPivot.forward;
            Gizmos.DrawLine(transform.position, transform.position + widthL * closeAttackRange);
            Gizmos.DrawLine(transform.position, transform.position + widthR * closeAttackRange);
        }
    }
}
