using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour,IDamageable
{
    [SerializeField]private float moveSpeed = 5f;
    [SerializeField]public int atk = 5;
    private CharacterController controller;
    
    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    void Update()
    {
        HandleMove();
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
    

    public void TakeDamage(float amount)
    {
        Debug.Log($"Player took {amount} damage!");
    }
}
