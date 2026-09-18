using UnityEngine;

namespace WitchGardenDemo
{
    // 2D 스프라이트가 항상 카메라를 정면으로 바라보도록 회전시킨다.
    // (부모(캐릭터 본체)가 회전해도 스프라이트 자체는 항상 카메라와 평행하게 유지된다.)
    public class Billboard : MonoBehaviour
    {
        private Camera targetCamera;

        void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null)
                {
                    return;
                }
            }

            transform.rotation = targetCamera.transform.rotation;
        }
    }
}
