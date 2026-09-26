using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;

public class PlayerAttack : MonoBehaviour
{
    public WeaponData weaponData;
    public float attackInterval = 1f; // 공격 간격
    private float lastAttackTime = -999f; // 마지막 공격 시간
    public float hitDetectionWidth = 15f;

    public PlayerController playerController; // 플레이어 컨트롤러 참조
    public DPMTracker dpmTracker; // DPM 트래커 참조

    public Transform weaponPivot; // 빈 오브젝트, Player 자식으로 배치
    private List<Transform> enemiesInRange = new List<Transform>(); // 감지된 적 리스트
 
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        dpmTracker = GetComponent<DPMTracker>();
    }
    void Update()
    {
        FindEnemy();
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            dpmTracker.ResetTracker();
        }
    }
    void OnEnable()
    {
        playerController.InputActions.Player.Atk.performed += OnAttackPerformed;
        playerController.InputActions.Player.Atk.performed += OnResetPerformed;
    }

    void OnDisable()
    {
        playerController.InputActions.Player.Atk.performed -= OnAttackPerformed;
        playerController.InputActions.Player.Atk.performed -= OnResetPerformed;
    }
    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        TryAttack();
    }

    private void OnResetPerformed(InputAction.CallbackContext ctx)
    {
        dpmTracker.ResetTracker();
    }

    void FindEnemy()
    {
        enemiesInRange.Clear(); 
        float sqrDetectRange = weaponData.atk_Range * weaponData.atk_Range;

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
    private float lastStartYaw;
    private float lastEndYaw;

    IEnumerator SwingAttack()
    {
        float facingSign = playerController.facingSign; // 1 or -1
        
        float startYaw = facingSign > 0 ? 90f - weaponData.Angle/2 : 270f + weaponData.Angle/2;
        //float startYaw = 0f;
        float endYaw = facingSign > 0 ? 90 + weaponData.Angle/2 : 270f - weaponData.Angle/2;
        lastStartYaw = startYaw; // Gizmo용으로 저장
        lastEndYaw = endYaw;
        Debug.Log($"startYaw: {startYaw}, endYaw: {endYaw}");

        HashSet<Transform> hit = new HashSet<Transform>();
        float elapsed = 0f;

        while (elapsed < weaponData.atk_1_Time)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / weaponData.atk_1_Time;
            float currentYaw = Mathf.Lerp(startYaw, endYaw, t);

            weaponPivot.rotation = Quaternion.Euler(0f, currentYaw, 0f);

            CheckWeaponHit(hit);

            yield return null;
        }
    }

    void CheckWeaponHit(HashSet<Transform> hit)
    {
        float sqrAttackRange = weaponData.atk_Range * weaponData.atk_Range;

        foreach (Transform enemy in enemiesInRange)
        {
            if (hit.Contains(enemy)) continue;

            Vector3 dirToEnemy = enemy.position - transform.position;
            if (dirToEnemy.sqrMagnitude > sqrAttackRange) continue;

            float angleToWeapon = Vector3.Angle(weaponPivot.forward, dirToEnemy);
             Debug.Log($"{enemy.name}: 각도차 {angleToWeapon:F1}도 (판정폭 {hitDetectionWidth})");
            if (angleToWeapon <= hitDetectionWidth) 
            {
                hit.Add(enemy);
                if (enemy.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(playerController.stats.base_Atk);
                    dpmTracker.RecordDamage(playerController.stats.base_Atk);
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
        Gizmos.DrawWireSphere(transform.position, weaponData.atk_Range);
        Vector3 forwardBoundary = Quaternion.Euler(0, weaponData.atk_Range, 0) * transform.forward;
        Vector3 backwardBoundary = Quaternion.Euler(0, -weaponData.atk_Range, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + forwardBoundary * weaponData.atk_Range);
        Gizmos.DrawLine(transform.position, transform.position + backwardBoundary * weaponData.atk_Range);

        if(weaponPivot != null)
        {
            Gizmos.color = Color.blue;
            Vector3 widthL = Quaternion.Euler(0, -hitDetectionWidth * 0.5f, 0) * weaponPivot.forward;
            Vector3 widthR = Quaternion.Euler(0, hitDetectionWidth * 0.5f, 0) * weaponPivot.forward;
            Gizmos.DrawLine(transform.position, transform.position + widthL * weaponData.atk_Range);
            Gizmos.DrawLine(transform.position, transform.position + widthR * weaponData.atk_Range);
        }

         // 스윙의 시작/끝 경계선을 항상 그려서 확인 가능하게
        Gizmos.color = Color.green; // 시작 각도
        Vector3 startDir = Quaternion.Euler(0, lastStartYaw, 0) * Vector3.forward;
        Gizmos.DrawLine(transform.position, transform.position + startDir * (weaponData.atk_Range + 2f));

        Gizmos.color = Color.magenta; // 끝 각도
        Vector3 endDir = Quaternion.Euler(0, lastEndYaw, 0) * Vector3.forward;
        Gizmos.DrawLine(transform.position, transform.position + endDir * weaponData.atk_Range);
    }
}
