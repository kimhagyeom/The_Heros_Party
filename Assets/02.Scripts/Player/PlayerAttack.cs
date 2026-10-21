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
    [SerializeField] private float comboResetTime = 0.5f;   // 스윙이 끝난 뒤 이 시간 안에 눌러야 다음 타로 이어짐
    [SerializeField] private float comboFinishDelay = 0.2f; // 마지막 타 후딜 (0이면 없음)
    private float lastSwingEndTime = -999f;                 // 마지막 스윙이 끝난 시각
    private bool attackBuffered = false;                    // 스윙 중에 눌린 공격 입력


    public PlayerController playerController; // 플레이어 컨트롤러 참조
    public DPMTracker dpmTracker; // DPM 트래커 참조
    private Animator animator;

    public Transform weaponPivot; // 빈 오브젝트, Player 자식으로 배치
    private List<Transform> enemiesInRange = new List<Transform>(); // 감지된 적 리스트

    private bool isAtk = false;
    public bool IsAttacking => isAtk;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        dpmTracker = GetComponent<DPMTracker>();
        animator = GetComponentInChildren<Animator>();
        currentWeaponData = mainWeaponData;

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
    void Update()
    {
        FindEnemy();
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            dpmTracker.ResetTracker();
        }
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

        foreach (Enemy enemy in EnemyRegistry.Instance.ActiveEnemies)
        {
            float range = currentWeaponData.atk_Range + GetEnemyRadius(enemy.transform);
            float sqrDistance = (enemy.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance <= range * range)
            {
                enemiesInRange.Add(enemy.transform);
            }
        }
    }
    void TryAttack()
    {
        if (playerController.IsCasting) return; // 스킬 사용 중에는 기본 공격 불가

        // 스윙 버그 수정
        if (isAtk)
        {
            attackBuffered = true;
            return;
        }
        if (Time.time - lastAttackTime < attackInterval) return;
        if (Time.time - lastSwingEndTime > comboResetTime)
        {
            comboIndex = 0;
        }
        lastAttackTime = Time.time;
        StartCoroutine(SwingAttack(comboIndex));
        comboIndex = (comboIndex + 1) % currentWeaponData.combo_Step.Length;
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
        PlayAttackAnimation(step);

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
        // 마지막 타면 후딜 (이 동안은 isAtk 유지 → 입력은 버퍼에만 쌓임)
        bool isLastStep = step == currentWeaponData.combo_Step.Length - 1;
        if (isLastStep && comboFinishDelay > 0f)
            yield return new WaitForSeconds(comboFinishDelay);
        lastSwingEndTime = Time.time;
        isAtk = false;

        // 스윙 중에 눌러둔 입력이 있으면 바로 다음 타
        if (attackBuffered)
        {
            attackBuffered = false;
            TryAttack();
        }
    }

    void CheckWeaponHit(HashSet<Transform> hit, float damageRate)
    {
        //float sqrAttackRange = currentWeaponData.atk_Range * currentWeaponData.atk_Range;


        foreach (Transform enemy in enemiesInRange)
        {
            if (hit.Contains(enemy)) continue;

            Vector3 dirToEnemy = enemy.position - transform.position;
            float range = currentWeaponData.atk_Range + GetEnemyRadius(enemy);
            if (dirToEnemy.sqrMagnitude > range * range) continue;

            float angleToWeapon = Vector3.Angle(weaponPivot.forward, dirToEnemy);
            if (angleToWeapon <= hitDetectionWidth) 
            {
                hit.Add(enemy);
                if (enemy.TryGetComponent<IDamageable>(out var damageable))
                {
                    DamageInfo damageInfo = new DamageInfo
                    {
                        damage = playerController.stats.base_Atk * damageRate,
                        infection = 0f,
                        hitDirection = (enemy.position - transform.position).normalized,
                        knockbackDistance = 0f
                    };
                    damageable.TakeDamage(damageInfo); // 캐릭터 공격력 * 무기 비율
                    dpmTracker.RecordDamage(playerController.stats.base_Atk * damageRate);
                }
            }
        }
    }
    float GetEnemyRadius(Transform enemy)
    {
        if (enemy.TryGetComponent<CapsuleCollider>(out var capsule))
            return capsule.radius * Mathf.Max(enemy.lossyScale.x, enemy.lossyScale.z);
        return 0f;
    }

    void SwitchWeapon()
    {
        if(isAtk)return;
        Debug.Log("무기 스위칭");
        currentWeaponData = currentWeaponData == mainWeaponData ? subWeaponData : mainWeaponData;
    }

    public void ResetAttack()
    {
        StopAllCoroutines();
        isAtk = false;
        attackBuffered = false;
        lastSwingEndTime = -999f;
        comboIndex = 0;
        currentWeaponData = mainWeaponData; //주무기로 되돌리는건데 (아직 보조무기 없어서..기획 후 수정 필요)
    }
    void PlayAttackAnimation(int step)
    {
        if (animator == null) return;
        int stateHash = Animator.StringToHash($"Player_Attack{step + 1}");
        if (animator.HasState(0, stateHash))
            animator.Play(stateHash, 0, 0f);
    }

    // void OnDrawGizmos()
    // {
    //     // Gizmos.color = Color.red;
    //     // Gizmos.DrawWireSphere(transform.position, detectRange);
    //     // Vector3 rightBoundary = Quaternion.Euler(0, attackAngle, 0) * transform.forward;
    //     // Vector3 leftBoundary = Quaternion.Euler(0, -attackAngle, 0) * transform.forward;
    //     // Gizmos.DrawLine(transform.position, transform.position + rightBoundary * detectRange);
    //     // Gizmos.DrawLine(transform.position, transform.position + leftBoundary * detectRange);

    //     Gizmos.color = Color.white;
    //     Gizmos.DrawWireSphere(transform.position, currentWeaponData.atk_Range);
    //     Vector3 forwardBoundary = Quaternion.Euler(0, currentWeaponData.Angle, 0) * transform.forward;
    //     Vector3 backwardBoundary = Quaternion.Euler(0, -currentWeaponData.Angle, 0) * transform.forward;
    //     Gizmos.DrawLine(transform.position, transform.position + forwardBoundary * currentWeaponData.atk_Range);
    //     Gizmos.DrawLine(transform.position, transform.position + backwardBoundary * currentWeaponData.atk_Range);

    //     if(weaponPivot != null)
    //     {
    //         Gizmos.color = Color.blue;
    //         Vector3 widthL = Quaternion.Euler(0, -hitDetectionWidth * 0.5f, 0) * weaponPivot.forward;
    //         Vector3 widthR = Quaternion.Euler(0, hitDetectionWidth * 0.5f, 0) * weaponPivot.forward;
    //         Gizmos.DrawLine(transform.position, transform.position + widthL * currentWeaponData.atk_Range);
    //         Gizmos.DrawLine(transform.position, transform.position + widthR * currentWeaponData.atk_Range);
    //     }

    //      // 스윙의 시작/끝 경계선을 항상 그려서 확인 가능하게
    //     Gizmos.color = Color.green; // 시작 각도
    //     Vector3 startDir = Quaternion.Euler(0, lastStartYaw, 0) * Vector3.forward;
    //     Gizmos.DrawLine(transform.position, transform.position + startDir * (currentWeaponData.atk_Range + 2f));

    //     Gizmos.color = Color.magenta; // 끝 각도
    //     Vector3 endDir = Quaternion.Euler(0, lastEndYaw, 0) * Vector3.forward;
    //     Gizmos.DrawLine(transform.position, transform.position + endDir * currentWeaponData.atk_Range);
    // }
}
