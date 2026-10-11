using System.Linq;
using UnityEngine;

namespace WitchGardenDemo
{
    // 2.5D 탑다운 시점을 위해 대상 위/뒤쪽에서 부드럽게 따라가는 카메라
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 10f, -6f);
        public float followSpeed = 10f;
        [SerializeField] private float amplitude = 0.5f;      // 흔들림 세기
        [SerializeField] private float shakeDuration = 0.2f;  // 지속시간 (amplitudeDuration 이름 변경)
        [SerializeField] private float frequency = 25f;       // 흔들림 빈도
        private float shakeTimer;        
        private Vector3 followPosition = Vector3.zero;
        private Vector3 shakeOffset;
        private void Start()
        {
            followPosition = transform.position;
        }

        void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            followPosition = Vector3.Lerp(followPosition, desiredPosition, followSpeed * Time.deltaTime);
            
            //transform.LookAt(target.position + Vector3.up);

            shakeOffset = Vector3.zero;

            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;

                float ratio = Mathf.Clamp01(shakeTimer / shakeDuration);

                float t = Time.time * frequency;
                float x = Mathf.PerlinNoise(t, 0f) * 2f - 1f;
                float y = Mathf.PerlinNoise(t, 100f) * 2f - 1f;

                shakeOffset = (transform.right * x + transform.up * y) * amplitude * ratio;
            }

            transform.position = followPosition + shakeOffset;
        }
        public void ShakeCamera()//float intensity, float duration
        {
            //amplitude = intensity;
            //shakeDuration = duration;
            shakeTimer = shakeDuration;
        }
    }
}
