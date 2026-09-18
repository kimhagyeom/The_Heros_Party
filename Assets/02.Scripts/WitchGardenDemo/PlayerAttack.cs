using UnityEngine;
using UnityEngine.InputSystem;

namespace WitchGardenDemo
{
    // 캐릭터 정면의 구 범위 판정으로 공격을 처리하는 기본 근접 공격
    // (이 프로젝트가 새 Input System을 사용하므로 Keyboard/Mouse.current로 입력을 읽는다.)
    public class PlayerAttack : MonoBehaviour
    {
        [Header("공격 설정")]
        public int attackDamage = 10;
        public float attackRadius = 1f;
        public float attackCooldown = 0.5f;
        public LayerMask enemyLayer;

        [Header("참조")]
        public Transform attackPoint; // 캐릭터 앞쪽 공격 판정 위치

        private float lastAttackTime = -999f;

        void Update()
        {
            bool attackPressed = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                                  (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame);

            if (attackPressed && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }

        private void Attack()
        {
            Vector3 origin = GetAttackOrigin();
            Collider[] hits = Physics.OverlapSphere(origin, attackRadius, enemyLayer);

            foreach (Collider hit in hits)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                damageable?.TakeDamage(attackDamage);
            }

            Debug.Log($"공격! 대상 {hits.Length}명 적중");
        }

        private Vector3 GetAttackOrigin()
        {
            return attackPoint != null ? attackPoint.position : transform.position + transform.forward;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GetAttackOrigin(), attackRadius);
        }
    }
}
