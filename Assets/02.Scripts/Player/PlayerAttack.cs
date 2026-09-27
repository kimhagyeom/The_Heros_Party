using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;

public class PlayerAttack : MonoBehaviour
{
    public WeaponData currentWeaponData;
    public WeaponData mainWeaponData;
    public WeaponData subWeaponData;
    public float attackInterval = 1f; // 공격 간격
    private float lastAttackTime = -999f; // 마지막 공격 시간
    public float hitDetectionWidth = 15f;
    private int comboIndex = 0;
    private float comboResetTime = 0.5f;

    public PlayerController playerController; // 플레이어 컨트롤러 참조
    public DPMTracker dpmTracker; // DPM 트래커 참조

    public Transform weaponPivot; // 빈 오브젝트, Player 자식으로 배치
    private List<Transform> enemiesInRange = new List<Transform>(); // 감지된 적 리스트

    public bool isAtk = false;
 
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        dpmTracker = GetComponent<DPMTracker>();
        currentWeaponData = mainWeaponData;
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
        playerController.InputActions.Player.DPMReset.performed += OnResetPerformed;
        playerController.InputActions.Player.WeaponSwitch.performed += OnWeaponSwitchPerformed;
    }

    void OnDisable()
    {
        playerController.InputActions.Player.Atk.performed -= OnAttackPerformed;
        playerController.InputActions.Player.DPMReset.performed -= OnResetPerformed;
        playerController.InputActions.Player.WeaponSwitch.performed -= OnWeaponSwitchPerformed;
    }
    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        TryAttack();
    }

    private void OnResetPerformed(InputAction.CallbackContext ctx)
    {
        dpmTracker.ResetTracker();
    }
    
    private void OnWeaponSwitchPerformed(InputAction.CallbackContext ctx)
    {
        SwitchWeapon();
    }

    void FindEnemy()
    {
        enemiesInRange.Clear(); 
        float sqrDetectRange = currentWeaponData.atk_Range * currentWeaponData.atk_Range;

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
        if (isAtk) return;
        if (Time.time - lastAttackTime < attackInterval) return;

        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboIndex = 0;
        }
        lastAttackTime = Time.time;
        StartCoroutine(SwingAttack(comboIndex));
        comboIndex = (comboIndex + 1) % currentWeaponData.combo_Step.Length;
        Debug.Log(comboIndex +  " comboIndex" + currentWeaponData.combo_Step.Length);
    }
    private float lastStartYaw;
    private float lastEndYaw;

    IEnumerator SwingAttack(int step)
    {
        WeaponData.ComboStep comboStep = currentWeaponData.combo_Step[step];
        float facingSign = playerController.facingSign;

        float baseStart = facingSign > 0 ? 90f - currentWeaponData.Angle / 2 : 270f + currentWeaponData.Angle / 2;
        float baseEnd = facingSign > 0 ? 90f + currentWeaponData.Angle / 2 : 270f - currentWeaponData.Angle / 2;

        // 짝수 타(0, 2타)는 baseStart→baseEnd, 홀수 타(1타)는 반대로
        float startYaw = (step % 2 == 0) ? baseStart : baseEnd;
        float endYaw = (step % 2 == 0) ? baseEnd : baseStart;
        //Debug.Log("startYaw : " + startYaw + "step : " + step);
        lastStartYaw = startYaw; // Gizmo용으로 저장
        lastEndYaw = endYaw;
        isAtk = true;
        Debug.Log($"startYaw: {startYaw}, endYaw: {endYaw}");

        HashSet<Transform> hit = new HashSet<Transform>();
        float elapsed = 0f;

        while (elapsed < comboStep.duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / comboStep.duration;
            float currentYaw = Mathf.Lerp(startYaw, endYaw, t);

            weaponPivot.rotation = Quaternion.Euler(0f, currentYaw, 0f);

            CheckWeaponHit(hit,comboStep.damageRate);

            yield return null;
        }
        isAtk = false;
    }

    void CheckWeaponHit(HashSet<Transform> hit, float damageRate)
    {
        float sqrAttackRange = currentWeaponData.atk_Range * currentWeaponData.atk_Range;

        foreach (Transform enemy in enemiesInRange)
        {
            if (hit.Contains(enemy)) continue;

            Vector3 dirToEnemy = enemy.position - transform.position;
            if (dirToEnemy.sqrMagnitude > sqrAttackRange) continue;

            float angleToWeapon = Vector3.Angle(weaponPivot.forward, dirToEnemy);
            if (angleToWeapon <= hitDetectionWidth) 
            {
                hit.Add(enemy);
                if (enemy.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(playerController.stats.base_Atk * damageRate); // 캐릭터 공격력 * 무기 비율
                    dpmTracker.RecordDamage(playerController.stats.base_Atk * damageRate);
                }
            }
        }
    }

    void SwitchWeapon()
    {
        if(isAtk)return;
        Debug.Log("무기 스위칭");
        currentWeaponData = currentWeaponData == mainWeaponData ? subWeaponData : mainWeaponData;
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
        Gizmos.DrawWireSphere(transform.position, currentWeaponData.atk_Range);
        Vector3 forwardBoundary = Quaternion.Euler(0, currentWeaponData.Angle, 0) * transform.forward;
        Vector3 backwardBoundary = Quaternion.Euler(0, -currentWeaponData.Angle, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + forwardBoundary * currentWeaponData.atk_Range);
        Gizmos.DrawLine(transform.position, transform.position + backwardBoundary * currentWeaponData.atk_Range);

        if(weaponPivot != null)
        {
            Gizmos.color = Color.blue;
            Vector3 widthL = Quaternion.Euler(0, -hitDetectionWidth * 0.5f, 0) * weaponPivot.forward;
            Vector3 widthR = Quaternion.Euler(0, hitDetectionWidth * 0.5f, 0) * weaponPivot.forward;
            Gizmos.DrawLine(transform.position, transform.position + widthL * currentWeaponData.atk_Range);
            Gizmos.DrawLine(transform.position, transform.position + widthR * currentWeaponData.atk_Range);
        }

         // 스윙의 시작/끝 경계선을 항상 그려서 확인 가능하게
        Gizmos.color = Color.green; // 시작 각도
        Vector3 startDir = Quaternion.Euler(0, lastStartYaw, 0) * Vector3.forward;
        Gizmos.DrawLine(transform.position, transform.position + startDir * (currentWeaponData.atk_Range + 2f));

        Gizmos.color = Color.magenta; // 끝 각도
        Vector3 endDir = Quaternion.Euler(0, lastEndYaw, 0) * Vector3.forward;
        Gizmos.DrawLine(transform.position, transform.position + endDir * currentWeaponData.atk_Range);
    }
}
