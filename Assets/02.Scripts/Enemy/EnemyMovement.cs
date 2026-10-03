using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyMovement : MonoBehaviour
{
    private float moveSpeed;

    void Start()
    {
        moveSpeed = GetComponent<Enemy>().Data.move_Speed;
    }

    public void Move(Vector3 dir)
    {
        dir.y = 0f;
        transform.position += dir.normalized * moveSpeed * Time.deltaTime;
    }

    public void LookAt(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}