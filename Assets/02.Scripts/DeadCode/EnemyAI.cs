using UnityEngine;

//테스트용 AI
//경직/넉백/슈퍼아머/둔화는 아직 미구현
[RequireComponent(typeof(Enemy))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Idle, Chase, Windup, Active, Recovery }

    [SerializeField] private State currentState = State.Idle;

    private Enemy enemy;
    private EnemyData data;
    private EnemyAtkData atk;
    private Transform target;
    private IDamageable targetDamageable;

    private float stateTimer;
    private float lastAttackTime = -999f;
    private Vector3 attackDir;   // 선딜 시작 시점에 고정한 공격 방향
    private bool hasHit;         // 판정 시간 중 한 번만 맞도록

    void Start()
    {
        enemy = GetComponent<Enemy>();
        data = enemy.Data;
        atk = (data.atk_List != null && data.atk_List.Length > 0) ? data.atk_List[0] : null;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            target = player.transform;
            targetDamageable = player;
        }
    }

    void Update()
    {
        if (enemy.IsDead || target == null) return;

        switch (currentState)
        {
            case State.Idle:     UpdateIdle();     break;
            case State.Chase:    UpdateChase();    break;
            case State.Windup:   UpdateWindup();   break;
            case State.Active:   UpdateActive();   break;
            case State.Recovery: UpdateRecovery(); break;
        }
    }

    void UpdateIdle()
    {
        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            ChangeState(State.Chase);
        }
    }

    void UpdateChase()
    {
        if (IsInAttackRange())
        {
            if (Time.time - lastAttackTime >= atk.cooldown)
            {
                ChangeState(State.Windup);
            }
            return;
        }

        Vector3 toTarget = FlatDirTo(target.position);
        if (toTarget.magnitude <= data.stop_Distance) return;

        transform.position += toTarget.normalized * data.move_Speed * Time.deltaTime;
    }

    void UpdateWindup()
    {
        stateTimer += Time.deltaTime;
        if (stateTimer >= atk.windup_Time)
        {
            ChangeState(State.Active);
        }
    }
    void UpdateActive()
    {
        stateTimer += Time.deltaTime;

        if (!hasHit && IsTargetInHitbox())
        {
            hasHit = true;
            targetDamageable.TakeDamage(atk.hp_Damage);
        }

        if (stateTimer >= atk.active_Time)
        {
            lastAttackTime = Time.time;
            ChangeState(State.Recovery);
        }
    }

    void UpdateRecovery()
    {
        stateTimer += Time.deltaTime;
        if (stateTimer >= atk.recovery_Time)
        {
            ChangeState(State.Chase);
        }
    }

    void ChangeState(State next)
    {
        currentState = next;
        stateTimer = 0f;

        if (next == State.Windup)
        {
            attackDir = FlatDirTo(target.position).normalized;
            hasHit = false;
        }
    }

    bool IsInAttackRange()
    {
        if (atk == null) return false;
        return FlatDirTo(target.position).sqrMagnitude <= atk.atk_Range * atk.atk_Range;
    }

    bool IsTargetInHitbox()
    {
        Vector3 toTarget = FlatDirTo(target.position);

        switch (atk.hitbox_Shape)
        {
            case Enemy_Hitbox_Shape.Sector:
                if (toTarget.sqrMagnitude > atk.radius * atk.radius) return false;
                return Vector3.Angle(attackDir, toTarget) <= atk.angle * 0.5f;

            case Enemy_Hitbox_Shape.Box:
                float forward = Vector3.Dot(toTarget, attackDir);
                float side = Vector3.Dot(toTarget, Vector3.Cross(Vector3.up, attackDir));
                return forward >= 0f && forward <= atk.length && Mathf.Abs(side) <= atk.width * 0.5f;
        }
        return false;
    }

    Vector3 FlatDirTo(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;
        dir.y = 0f;
        return dir;
    }

    void OnDrawGizmosSelected()
    {
        if (atk == null) return;

        Gizmos.color = Color.yellow; // 공격 가능 거리
        Gizmos.DrawWireSphere(transform.position, atk.atk_Range);

        if (currentState == State.Windup || currentState == State.Active)
        {
            Gizmos.color = currentState == State.Active ? Color.red : new Color(1f, 0.5f, 0f);
            Gizmos.DrawLine(transform.position, transform.position + attackDir * Mathf.Max(atk.radius, atk.length));
        }
    }
}
