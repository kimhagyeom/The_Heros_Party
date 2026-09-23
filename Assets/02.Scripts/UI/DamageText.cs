using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.5f; // 위로 올라가는 속도
    [SerializeField] private float lifeTime = 0.8f;  // 사라지기까지 걸리는 시간

    private TextMeshPro text;
    private Color startColor;
    private float timer;
    private Camera cam;

    void Awake()
    {
        text = GetComponent<TextMeshPro>();
        startColor = text.color;
        cam = Camera.main;
    }

    // 생성 직후 Enemy가 호출해서 숫자를 넣어줌
    public void Setup(float damage)
    {
        text.text = ((int)damage).ToString();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 위로 이동
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 점점 투명하게
        float alpha = 1f - (timer / lifeTime);
        text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    void LateUpdate()
    {
        // 항상 카메라를 바라보게 함
        if (cam != null)
        {
            transform.rotation = cam.transform.rotation;
        }
    }
}