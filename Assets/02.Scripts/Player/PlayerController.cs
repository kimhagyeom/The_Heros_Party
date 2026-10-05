using System.Collections;
using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System;
using Unity.VisualScripting;


[System.Serializable]
public class PlayerStats
{
    public int Character_ID = 1001;
    public string Character_Name = "아르마";
    public float maxHealth = 100f;
    public float startHealth = 100f;
    public float Move_Speed = 7f;
    public float maxStamina = 100f;
    public float startStamina = 100f;
    public float stamina_Regen = 12.5f;
    public float stamina_Regen_Delay = 1f;
    public float max_Infection = 100f;
    public float start_Infection = 0f;
    public float hit_Invincible_Time = 0.5f;
    public float dash_Stamina_Cost = 50f;
    //대시 거리 = dashSpeed * dashDuration
    public float dash_Distance = 5f;
    public float dashDuration = 4f;
    public float dash_Invincible_Duration = 0.2f;
    public float base_Atk = 10f;


    public float currentStamina;
    public float currentInfection;

    public void Init()
    {
        currentStamina = maxStamina;
        currentInfection = start_Infection;
    }
}

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
    private bool isDash = false;
    public float facingSign;

    private Coroutine regenRoutine;
    private Coroutine knockbackRoutine;
    public bool IsKnockback { get; private set; }


    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        if (mainCamera == null) mainCamera = Camera.main;
        stats.Init();

        health = GetComponent<Health>();

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
    void Update()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        // 넉백 중에는 입력 이동 무시
        if (!IsKnockback)
            controller.SimpleMove(moveDirection * stats.Move_Speed);

        bool isMoving = !IsKnockback && moveDirection.sqrMagnitude > 0.0001f;
        if (animator != null)
        {
            animator.SetBool(AnimHash.IsMoving, isMoving);
        }

        HandleFacing();
    }

    private void HandleFacing()
    {
        Mouse mouse = Mouse.current;
        if(mouse == null) return;

        Vector2 mousePos = mouse.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);
            Vector3 directionToMouse = mouseWorldPos - transform.position;
            directionToMouse.y = 0f; // 수직 방향 무시

            if(directionToMouse.sqrMagnitude > 0.0001f)
            {
                transform.forward = directionToMouse.normalized;
                if(spriteVisual != null)
                {
                    spriteVisual.rotation = Quaternion.identity;

                    facingSign = directionToMouse.x >= 0 ? 1f : -1f;

                    Vector3 localScale = spriteVisual.localScale;
                    spriteVisual.localScale = new Vector3(Mathf.Abs(localScale.x) * facingSign, localScale.y, localScale.z);
                }
            }
        }

    }
    private void HandleDodge()
    {
        if(isDash || stats.currentStamina < stats.dash_Stamina_Cost) return;

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
        health.AddInvincible(stats.dashDuration); // 지금은 대쉬 전체 시간 동안 무적

        Vector3 direction = transform.forward;
        float elapsed = 0f;

        while(elapsed < stats.dashDuration)
        {
            elapsed += Time.deltaTime;
            controller.Move(direction.normalized * stats.dash_Distance * Time.deltaTime);
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