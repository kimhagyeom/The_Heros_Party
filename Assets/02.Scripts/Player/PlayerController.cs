using System.Collections;
using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System;
using Unity.VisualScripting;

// 입력, 이동, 바라보는 방향, 대시, 넉백 실행 담당
// 피격 반응(데미지 텍스트, 무적, 감염도, 사망)은 Player가 담당
[RequireComponent(typeof(Health))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform spriteVisual;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float knockbackDuration = 0.2f;
    private PlayerInputActions inputActions; // 실제 값을 저장하는 필드 (소문자, private)

    public PlayerInputActions InputActions => inputActions;
    private Vector2 moveInput;
    public PlayerStats stats = new PlayerStats();
    private CharacterController controller;
    private Animator animator;
    private Health health;   // 대시 무적에 사용
    private PlayerAttack playerAttack; // 공격 중 여부 확인용
    private PlayerSkillController skillController; // 스킬 사용 중 여부 확인용
    private bool isDash = false;
    public float facingSign;

    private Coroutine regenRoutine;
    private Coroutine knockbackRoutine;

    // 플레이어 행동 상태 / 공격이나 스킬을 시작할 수 있는지 판단할 때 IsBusy 하나만 보면 되도록 모아둠/
    // 새 행동(스킬 등)이 생기면 IsBusy에 추가!!
    public bool IsKnockback { get; private set; }
    public bool IsDashing => isDash;
    public bool IsAttacking => playerAttack != null && playerAttack.IsAttacking;
    public bool IsCasting => skillController != null && skillController.IsCasting;
    public bool IsBusy => IsKnockback || IsDashing || IsAttacking || IsCasting;


    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        if (mainCamera == null) mainCamera = Camera.main;
        stats.Init();

        health = GetComponent<Health>();
        playerAttack = GetComponent<PlayerAttack>();
        skillController = GetComponent<PlayerSkillController>();

        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Dodge.performed += OnDodgePerformed;
        inputActions.Player.Enable();
    }
    void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Dodge.performed -= OnDodgePerformed;
        inputActions.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }
    private void OnDodgePerformed(InputAction.CallbackContext ctx)
    {
        HandleDodge();
    }
    private Vector3 moveDir = new Vector3();
    void Update()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        moveDir = moveDirection;

        // 넉백 중, 스킬 사용 중에는 입력 이동 무시
        bool canMove = !IsKnockback && !IsCasting;
        if (canMove)
            controller.SimpleMove(moveDirection * stats.Move_Speed);

        bool isMoving = canMove && moveDirection.sqrMagnitude > 0.0001f;
        if (animator != null)
        {
            animator.SetBool(AnimHash.IsMoving, isMoving);
        }

        // 스킬 사용 중에는 시작할 때 방향 유지 (마우스를 돌려도 안 바뀜)
        if (!IsCasting)
            HandleFacing();
    }

    //마우스 커서가 가리키는 위치 (바라보는 방향, 스킬 대상 찾기에 사용)
    public bool TryGetMouseWorldPosition(out Vector3 mouseWorldPos)
    {
        mouseWorldPos = Vector3.zero;

        Mouse mouse = Mouse.current;
        if(mouse == null) return false;

        Vector2 mousePos = mouse.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (!groundPlane.Raycast(ray, out float distance)) return false;

        mouseWorldPos = ray.GetPoint(distance);
        return true;
    }

    public void FaceDirection(Vector3 direction)
    {
        direction.y = 0f;
        if(direction.sqrMagnitude <= 0.0001f) return;

        transform.forward = direction.normalized;
        if(spriteVisual != null)
        {
            spriteVisual.rotation = Quaternion.identity;

            facingSign = direction.x >= 0 ? 1f : -1f;

            Vector3 localScale = spriteVisual.localScale;
            spriteVisual.localScale = new Vector3(Mathf.Abs(localScale.x) * facingSign, localScale.y, localScale.z);
        }
    }

    // 순간이동: CharacterController가 켜져 있으면 위치를 직접 바꿔도 되돌려지므로 잠깐 끄고 이동
    public void Teleport(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        controller.enabled = true;
    }

    private void HandleFacing()
    {
        if (TryGetMouseWorldPosition(out Vector3 mouseWorldPos))
            FaceDirection(mouseWorldPos - transform.position);
    }
    private void HandleDodge()
    {
        // 스킬 사용 중에는 대시 불가 (스킬 도중 위치가 바뀌지 않도록)
        if(isDash || IsCasting || stats.currentStamina < stats.dash_Stamina_Cost) return;

        stats.currentStamina -= stats.dash_Stamina_Cost;
        stats.currentStamina = Mathf.Clamp(stats.currentStamina, 0, stats.maxStamina);

        StartCoroutine(DashRoutine());

        if(regenRoutine != null) StopCoroutine(regenRoutine);
        regenRoutine = StartCoroutine(RegenStamina());
    }
    IEnumerator RegenStamina()
    {
        yield return new WaitForSeconds(stats.stamina_Regen_Delay);

        while(stats.currentStamina < stats.maxStamina)
        {
            stats.currentStamina += stats.stamina_Regen * Time.deltaTime; // 초당 회복량
            stats.currentStamina = Mathf.Clamp(stats.currentStamina, 0, stats.maxStamina);
            yield return null;
        }
        regenRoutine = null;
    }
    IEnumerator DashRoutine()
    {
        isDash = true;

        Vector3 direction = transform.forward;
        direction.y = 0f;
        direction.Normalize();

        // dashDuration 동안 dash_Distance만큼 가도록 속도 계산 (넉백과 같은 방식)
        float speed = stats.dashDuration > 0f ? stats.dash_Distance / stats.dashDuration : 0f;
        bool invincibleApplied = false;
        float elapsed = 0f;

        while(elapsed < stats.dashDuration)
        {
            // 무적은 시작 시간이 지난 시점에 한 번만 적용
            if (!invincibleApplied && elapsed >= stats.dash_Invincible_Start)
            {
                health.AddInvincible(stats.dash_Invincible_Duration);
                invincibleApplied = true;
            }

            // 마지막 프레임은 남은 시간만큼만 이동해서 거리 초과 방지
            float step = Mathf.Min(Time.deltaTime, stats.dashDuration - elapsed);
            controller.Move(moveDir * speed * step);
            elapsed += step;
            yield return null;
        }

        isDash = false;
    }

    // Player가 호출: 방향으로 거리만큼 짧은 시간 동안 밀림
    public void Knockback(Vector3 dir, float distance)
    {
        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);

        knockbackRoutine = StartCoroutine(KnockbackCoroutine(dir, distance));
    }

    IEnumerator KnockbackCoroutine(Vector3 dir, float distance)
    {
        IsKnockback = true;

        dir.y = 0f;
        dir.Normalize();

        float speed = distance / knockbackDuration;
        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            controller.Move(dir * speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        IsKnockback = false;
        knockbackRoutine = null;
    }

    IEnumerator Increase_Infection(float amount) // 정신감염도 상승
    {
        while (true)
        {
            stats.currentInfection += Time.deltaTime * amount;
            stats.currentInfection = Mathf.Clamp(stats.currentInfection, stats.start_Infection, stats.max_Infection);
            if(stats.currentInfection >= stats.max_Infection)
            {
                // TODO: 사망 처리는 Player 담당. 이 코루틴을 쓸 때 Player로 옮기기
                yield break;
            }

            yield return null;
        }
    }
}