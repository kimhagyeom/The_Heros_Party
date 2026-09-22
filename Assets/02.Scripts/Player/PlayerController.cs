using System.Collections;
using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System;


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
    public float dashSpeed = 5f;
    public float dashDuration = 4f;
    public float dash_Invincible_Duration = 0.2f;
    public float base_Attack = 10f;


    public float currentHealth;
    public float currentStamina;

    public void Init()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }
}
public class PlayerController : MonoBehaviour,IDamageable
{
    [SerializeField] private Transform spriteVisual;
    [SerializeField] private Camera mainCamera;
    public PlayerStats stats = new PlayerStats();
    [SerializeField] private int maxPlayerMentalPoint = 100;
    [SerializeField] private int currentPlayerMentalPoint = 100;
    private CharacterController controller;
    private Animator animator;
    private bool isDash = false;
    private bool isInvincible = false;

    private Coroutine regenRoutine;
    
    
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>(); 
        if (mainCamera == null) mainCamera = Camera.main;
        stats.Init();
        currentPlayerMentalPoint = maxPlayerMentalPoint;

    }
    void Update()
    {
        HandleMove();
        HandleFacing();
    }
         
    private void HandleMove()
    {
        if(isDash) return;
        Keyboard keyboard = Keyboard.current;
        float h = 0f;
        float v = 0f;

        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) h -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) h += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) v -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) v += 1f;
            if (Keyboard.current.spaceKey.wasPressedThisFrame) HandleDodge();
        }

        Vector3 moveDirection = new Vector3(h, 0f, v);
        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        controller.SimpleMove(moveDirection * stats.Move_Speed);
        
        bool isMoving = moveDirection.magnitude > 0.0001f;
        if (animator != null)
        {
            animator.SetBool(AnimHash.IsMoving, isMoving);
        }

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
                
                    float sign = directionToMouse.x >= 0 ? 1f : -1f;
                    Vector3 localScale = spriteVisual.localScale;
                    spriteVisual.localScale = new Vector3(Mathf.Abs(localScale.x) * sign, localScale.y, localScale.z);
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
        isInvincible = true;
        
        Vector3 direction = transform.forward;
        float elapsed = 0f;

        while(elapsed < stats.dashDuration)
        {
            elapsed += Time.deltaTime;
            controller.Move(direction.normalized * stats.dashSpeed * Time.deltaTime);
            yield return null;
        }

        isDash = false;
        isInvincible = false;        
    }

    public void TakeDamage(float amount)
    {
        if(isInvincible) return;
        Debug.Log($"Player took {amount} damage!");
        stats.currentHealth -= amount;
        if(stats.currentHealth <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        Debug.Log("Player has died!");
        // 게임 오버 처리 로직 추가 가능
    }
}
