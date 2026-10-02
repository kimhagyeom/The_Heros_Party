using UnityEngine;

// 테스트용 AI: 추적 → 선딜 → 판정 1회 → 후딜 → 추적
// 경직/넉백/슈퍼아머/둔화는 아직 미구현
[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(EnemyAttack))]
public class EnemyAI_S : MonoBehaviour
{
    private enum State { Chase, Windup, Recovery }

    [SerializeField] private State currentState = State.Chase;
    [SerializeField] private AttackTelegraph telegraph; // 자식 Telegraph 오브젝트 드래그

    private Enemy enemy;
    private EnemyAttack enemyAttack;
    private EnemyData data;
    private EnemyAtkData atk;
    private Transform target;

    private float stateTimer;
    private float lastAttackTime = -999f;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        enemyAttack = GetComponent<EnemyAttack>();
        data = enemy.Data;

        if (data.atk_List != null && data.atk_List.Length > 0)
            atk = data.atk_List[0];

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            target = player.transform;
            enemyAttack.SetTargetMask(1 << player.gameObject.layer);
        }
    }

    void Update()
    {
        if (enemy.IsDead)
        {
            HideTelegraph(); // 선딜 중에 죽으면 예고도 끄기
            return;
        }
        if (target == null || atk == null) return;

        switch (currentState)
        {
            case State.Chase:    UpdateChase();    break;
            case State.Windup:   UpdateWindup();   break;
            case State.Recovery: UpdateRecovery(); break;
        }
    }

    // 공격 거리 밖이면 다가가고, 안이면 쿨타임 확인 후 공격 시작
    void UpdateChase()
    {
        Vector3 toTarget = FlatDirTo(target.position);

        if (toTarget.sqrMagnitude <= atk.atk_Range * atk.atk_Range)
        {
            if (Time.time - lastAttackTime >= atk.cooldown)
                StartWindup(toTarget);
            return;
        }

        transform.position += toTarget.normalized * data.move_Speed * Time.deltaTime;
    }

    // 선딜 시작: 플레이어 쪽으로 몸을 돌려 공격 방향 고정
    void StartWindup(Vector3 toTarget)
    {
        if (toTarget.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(toTarget);

        // TODO: 데이터에 Show_Attack_Area가 추가되면 조건으로 걸기
        if (telegraph != null)
            telegraph.Show(atk);

        ChangeState(State.Windup);
    }

    // 선딜이 끝나는 순간 한 번만 판정
    void UpdateWindup()
    {
        stateTimer += Time.deltaTime;

        if (telegraph != null)
            telegraph.SetProgress(stateTimer / atk.windup_Time);

        if (stateTimer < atk.windup_Time) return;

        HideTelegraph();
        enemyAttack.Attack(atk);
        lastAttackTime = Time.time;
        ChangeState(State.Recovery);
    }

    // 후딜 동안 아무것도 안 하고, 끝나면 다시 추적
    void UpdateRecovery()
    {
        stateTimer += Time.deltaTime;
        if (stateTimer >= atk.recovery_Time)
            ChangeState(State.Chase);
    }

    void HideTelegraph()
    {
        if (telegraph != null)
            telegraph.Hide();
    }

    void ChangeState(State next)
    {
        currentState = next;
        stateTimer = 0f;
    }

    Vector3 FlatDirTo(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;
        dir.y = 0f;
        return dir;
    }

    void OnDrawGizmosSelected()
    {
        // 플레이 전에도 보이도록, atk가 없으면 Enemy 데이터에서 직접 가져옴
        EnemyAtkData a = atk;
        if (a == null)
        {
            Enemy e = GetComponent<Enemy>();
            if (e == null || e.Data == null || e.Data.atk_List == null || e.Data.atk_List.Length == 0) return;
            a = e.Data.atk_List[0];
        }

        // 공격 가능 거리 (노란 원)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, a.atk_Range);

        // 실제 판정 범위 (선딜 중이면 빨강, 평소엔 주황)
        Gizmos.color = currentState == State.Windup ? Color.red : new Color(1f, 0.5f, 0f);

        switch (a.hitbox_Shape)
        {
            case HitboxShape.Box:
                DrawBoxGizmo(a.width, a.length);
                break;
            case HitboxShape.Sector:
                DrawSectorGizmo(a.radius, a.angle);
                break;
        }
    }

    void DrawBoxGizmo(float width, float length)
    {
        Vector3 center = transform.position + transform.forward * (length / 2f);
        Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(width, 2f, length));
        Gizmos.matrix = Matrix4x4.identity; // 다른 기즈모에 영향 안 가게 원래대로
    }

    void DrawSectorGizmo(float radius, float angle)
    {
        const int segments = 24;
        Vector3 origin = transform.position;
        Vector3 start = Quaternion.Euler(0f, -angle / 2f, 0f) * transform.forward;
        Vector3 prev = origin + start * radius;

        if (angle < 360f) Gizmos.DrawLine(origin, prev); // 왼쪽 가장자리

        for (int i = 1; i <= segments; i++)
        {
            float step = -angle / 2f + angle * i / segments;
            Vector3 next = origin + (Quaternion.Euler(0f, step, 0f) * transform.forward) * radius;
            Gizmos.DrawLine(prev, next); // 호
            prev = next;
        }

        if (angle < 360f) Gizmos.DrawLine(origin, prev); // 오른쪽 가장자리
    }
}