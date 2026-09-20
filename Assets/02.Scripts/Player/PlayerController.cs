using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour,IDamageable
{
    [SerializeField] private Transform spriteVisual;
    [SerializeField] private Camera mainCamera;
    [SerializeField]private float moveSpeed = 5f;
    [SerializeField]public int atk = 5;
    [SerializeField] private float maxPlayerHealth = 100f;
    private float currentPlayerHealth;
    private CharacterController controller;
    
    
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (mainCamera == null) mainCamera = Camera.main;
        currentPlayerHealth = maxPlayerHealth;
    }
    void Update()
    {
        HandleMove();
        HandleFacing();
    }


    private void HandleMove()
    {
        Keyboard keyboard = Keyboard.current;
        float h = 0f;
        float v = 0f;

        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) h -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) h += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) v -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) v += 1f;
        }

        Vector3 moveDirection = new Vector3(h, 0f, v);
        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        controller.SimpleMove(moveDirection * moveSpeed);
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

    public void TakeDamage(float amount)
    {
        Debug.Log($"Player took {amount} damage!");
        currentPlayerHealth -= amount;
        if(currentPlayerHealth <= 0)
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
