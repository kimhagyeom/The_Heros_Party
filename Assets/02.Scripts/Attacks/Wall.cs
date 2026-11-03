using UnityEngine;

public class Wall : MonoBehaviour
{
    [SerializeField] private float growSpeed;

    private float speed;
    private float lifeTime;
    private Vector3 wall_Scale;

    public void Init(float speed, float distance)
    {
        this.speed = speed;
        lifeTime = speed > 0f ? distance / speed : 0f; // 이동거리만큼 가면 사라짐
        wall_Scale = transform.localScale;
    }

    void Update()
    {
        Move();
        wall_Scale.x += growSpeed * Time.deltaTime;
        transform.localScale = wall_Scale;
        
    }
    void Move()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
            Destroy(gameObject);
    }
    // 파괴 불가 장벽: 겹친 플레이어를 벽 진행 방향으로 밀어냄
    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
            player.Push(transform.forward * speed * Time.deltaTime);
    }
}
