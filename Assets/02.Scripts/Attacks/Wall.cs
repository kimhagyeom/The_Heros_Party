using UnityEngine;

public class Wall : MonoBehaviour
{
    private float speed;
    private float distance;
    private Vector3 wall_Scale;
    private float lifeTime;
    [SerializeField] private float growSpeed;
    public void Init(float speed, float distance, float lifeTime)
    {
        this.speed = speed;
        this.distance = distance;
        this.lifeTime = lifeTime;
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
        float step = speed * Time.deltaTime;

        transform.position += transform.forward * step;

        lifeTime -= Time.deltaTime;
        if(lifeTime <= 0f)
            Destroy(gameObject);
    }
}
