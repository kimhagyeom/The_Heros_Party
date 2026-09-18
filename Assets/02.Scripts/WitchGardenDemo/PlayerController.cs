using UnityEngine;
using UnityEngine.InputSystem;

namespace WitchGardenDemo
{
    // 탑다운 2.5D 이동 : WASD/방향키로 이동, 마우스 커서 방향으로 캐릭터(본체)가 회전하고
    // 2D 스프라이트는 Billboard 스크립트가 항상 카메라를 향하도록 유지하며 좌우로만 뒤집힌다.
    // (이 프로젝트가 새 Input System을 사용하므로 Keyboard/Mouse.current로 입력을 읽는다.)
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("이동 설정")]
        public float moveSpeed = 6f;
        public float rotationSpeed = 720f; // 초당 회전 각도

        [Header("참조")]
        public Camera mainCamera;
        public SpriteRenderer spriteRenderer; // 좌우 반전 처리 대상

        private CharacterController controller;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
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
            if (mainCamera == null || Mouse.current == null)
            {
                return;
            }

            // 마우스 커서가 가리키는 바닥 위 지점을 향해 캐릭터 본체를 회전시킨다.
            // (본체 회전은 공격 판정 방향 계산에 쓰이고, 스프라이트 자체는 Billboard가 따로 처리한다.)
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 targetPoint = ray.GetPoint(distance);
                Vector3 lookDirection = targetPoint - transform.position;
                lookDirection.y = 0f;

                if (lookDirection.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                    UpdateSpriteFlip(lookDirection);
                }
            }
        }

        private void UpdateSpriteFlip(Vector3 lookDirection)
        {
            if (spriteRenderer == null || mainCamera == null)
            {
                return;
            }

            // 카메라의 오른쪽 축을 기준으로 조준 방향이 좌/우 어느 쪽인지 판정해 스프라이트를 뒤집는다.
            Vector3 camRight = mainCamera.transform.right;
            camRight.y = 0f;

            if (camRight.sqrMagnitude < 0.0001f)
            {
                return;
            }

            camRight.Normalize();
            float side = Vector3.Dot(lookDirection.normalized, camRight);

            if (Mathf.Abs(side) > 0.1f)
            {
                spriteRenderer.flipX = side < 0f;
            }
        }
    }
}
