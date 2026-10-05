using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Enemy))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float knockbackDuration = 0.2f;

    private EnemyData enemyData;
    private float moveSpeed;

    private Coroutine knockbackRoutine;
    public bool IsKnockback { get; private set; }

    void Awake()
    {
        enemyData = GetComponent<Enemy>().Data;   
        moveSpeed = enemyData.move_Speed;
    }

    public void Move(Vector3 dir)
    {
        if (IsKnockback) return;  // 밀리는 중엔 추적 이동 무시

        dir.y = 0f;
        transform.position += dir.normalized * moveSpeed * Time.deltaTime;
    }

    public void LookAt(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    public void OnHit(DamageInfo info)
    {
        if (!enemyData.can_Knockback) return;

        if (knockbackRoutine != null)             // 이미 밀리는 중이면 멈추고 새로 시작
            StopCoroutine(knockbackRoutine);

        knockbackRoutine = StartCoroutine(KnockbackCoroutine(info.hitDirection, enemyData.knockback_Resistance));
    }

    IEnumerator KnockbackCoroutine(Vector3 dir, float distance)
    {
        IsKnockback = true;

        dir.y = 0f;
        dir.Normalize();

        float knockbackSpeed = distance / knockbackDuration;
        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            transform.position += dir * knockbackSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        IsKnockback = false;
        knockbackRoutine = null;
    }
}