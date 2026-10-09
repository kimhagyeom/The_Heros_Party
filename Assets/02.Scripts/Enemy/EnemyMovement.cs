using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(Enemy))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float knockbackDuration = 0.2f;

    private EnemyData enemyData;
    private NavMeshAgent agent;
    private float moveSpeed;

    private Coroutine knockbackRoutine;
    public bool IsKnockback { get; private set; }

    void Awake()
    {
        enemyData = GetComponent<Enemy>().Data;   
        agent = GetComponent<NavMeshAgent>();
        moveSpeed = enemyData.move_Speed;

        agent.speed = enemyData.move_Speed;
        agent.updateRotation = false; // 회전은 LookAt/flip으로 처리
    }

    public void MoveTo(Vector3 targetPos)
    {
        if (IsKnockback || !agent.enabled || !agent.isOnNavMesh) return;  // 밀리는 중에는 이동 입력 무시

        agent.isStopped = false;
        agent.SetDestination(targetPos);
    }
    public void Stop()
    {
         if (!agent.enabled || !agent.isOnNavMesh) return;

        agent.isStopped = true;
        agent.ResetPath();
    }
    // 사망 시 호출
    public void Disable()
    {
        Stop();
        agent.enabled = false;
    }

    public void LookAt(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    
    public void KnockBack(DamageInfo info)
    {
        if(!agent.enabled) return;

        if (knockbackRoutine != null)             // 이미 밀리는 중이면 멈추고 새로 시작
            StopCoroutine(knockbackRoutine);

        knockbackRoutine = StartCoroutine(KnockbackCoroutine(info.hitDirection, enemyData.knockback_Resistance));
    }

    IEnumerator KnockbackCoroutine(Vector3 dir, float distance)
    {
        IsKnockback = true;
        Stop();

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