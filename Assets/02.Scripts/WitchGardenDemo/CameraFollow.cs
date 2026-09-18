using UnityEngine;

namespace WitchGardenDemo
{
    // 2.5D 탑다운 시점을 위해 대상 위/뒤쪽에서 부드럽게 따라가는 카메라
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 10f, -6f);
        public float followSpeed = 10f;

        void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up);
        }
    }
}
