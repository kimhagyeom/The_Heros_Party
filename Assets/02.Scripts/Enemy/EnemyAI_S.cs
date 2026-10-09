using UnityEngine;
using System.Collections.Generic; 

// 테스트용 AI: 추적 → 선딜 → 판정 1회 → 후딜 → 추적
// 슈퍼아머/둔화는 아직 미구현
[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(EnemyAttack))]
[RequireComponent(typeof(EnemyMovement))]
public class EnemyAI_S : MonoBehaviour
{
    private enum State { Chase, Windup, Recovery, ChainDelay, Stagger}

    [Header("디버그 / 연결")]
    [SerializeField] private State currentState = State.Chase;
    [SerializeField] private AttackTelegraph telegraph; // 자식 Telegraph 오브젝트 드래그

    // 컴포넌트
    private Enemy enemy;
    private EnemyAttack enemyAttack;
    private EnemyMovement movement;

    // 데이터
    private EnemyData data;
    private EnemyAtkData atk;
    private EnemyAtkData[] atkList => data.atk_List;
    private List<EnemyAtkData> canAttackList = new List<EnemyAtkData>();
    private Dictionary<EnemyAtkData, float> lastUsedTime = new Dictionary<EnemyAtkData, float>();
    private Dictionary<EnemyAtkData,EnemyAtkData> chainMap = new Dictionary<EnemyAtkData, EnemyAtkData>();
    private Transform target;
    private Vector3 targetPos;

    //Movement
    private float repathTimer;
    private const float RepathInterval = 0.2f;  
    // 상태
    private float stateTimer;
    private float staggerDuration;public bool IsSuperArmor =>
                    currentState == State.Windup && atk != null && atk.has_Super_Armor;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        enemyAttack = GetComponent<EnemyAttack>();
        movement = GetComponent<EnemyMovement>();
        data = enemy.Data;
        BuildChainMap();

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
            movement.Disable();
            return;
        }
        if (target == null || atk == null) return;

        switch (currentState)
        {
            case State.Chase:    UpdateChase();    break;
            case State.Windup:   UpdateWindup();   break;
            case State.ChainDelay: UpdateChainDelay(); break;
            case State.Recovery: UpdateRecovery(); break;
            case State.Stagger: UpdateStagger();  break; 
        }
    }
    void BuildChainMap()
    {
        chainMap.Clear();
        foreach (EnemyAtkData a in atkList)
        {
            if(a.required_Atk_ID == 0) continue;
            foreach(EnemyAtkData prev in atkList)
            {
                if(prev.atk_ID == a.required_Atk_ID)
                {
                    chainMap[prev] = a;
                    break;
                }
            }
        }
    }

    // 공격 거리 밖이면 다가가고, 안이면 쿨타임 확인 후 공격 시작
    void UpdateChase()
    {
        Vector3 toTarget = FlatDirTo(target.position);

        CheckAttackList(toTarget);// 공격 가능 목록 갱신
        
        if(canAttackList.Count > 0)
        {
            StartWindup(toTarget);
            return;
        }
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = RepathInterval;
            movement.MoveTo(target.position);
        }  
    }
    private void CheckAttackList(Vector3 toTarget)
    {
        canAttackList.Clear();
        float distSqr = toTarget.sqrMagnitude;
        
        foreach (EnemyAtkData a in atkList)
        {
            if (a == null) continue;
            if (a.required_Atk_ID > 0) continue;
            if (distSqr > a.atk_Range * a.atk_Range) continue;     // 사거리 밖

            if (lastUsedTime.TryGetValue(a, out float last) &&
                Time.time - last < a.cooldown) continue;           // 쿨타임 중

            canAttackList.Add(a);
        }
    }

    void StartWindup(Vector3 toTarget)
    {
        movement.Stop();
        movement.LookAt(toTarget);
        targetPos = target.position; // 선딜 시작 시점의 위치를 기록, 선딜 중에 이동해도 그 위치로 공격
        if(canAttackList.Count > 0)
        {
            // 공격 가능 목록에서 랜덤 선택
            int index = Random.Range(0, canAttackList.Count);
            atk = canAttackList[index];
        }
        if (telegraph != null)
            telegraph.Show(atk, targetPos);

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
        enemyAttack.Attack(atk, targetPos);
        lastUsedTime[atk] = Time.time;
        if(chainMap.TryGetValue(atk, out EnemyAtkData nextAtk))
        {
            // 체인 공격이 있으면 다음 공격으로 교체
            atk = nextAtk;
            ChangeState(State.ChainDelay);
        }
        else
        {
            ChangeState(State.Recovery);
        }
    }
    void UpdateChainDelay()
    {
        stateTimer += Time.deltaTime;
        if (stateTimer < atk.chain_Delay) return;

        if (telegraph != null)
            telegraph.Show(atk, targetPos);
        ChangeState(State.Windup);
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
        if (next == State.Chase) repathTimer = 0f;
    }

    Vector3 FlatDirTo(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;
        dir.y = 0f;
        return dir;
    }

    private void UpdateStagger()
    {
        if(movement.IsKnockback) return; // 넉백 중이면 경직 상태 유지
        stateTimer += Time.deltaTime ;
        if(stateTimer >= staggerDuration)
            ChangeState(State.Chase);
    }
    public void Stagger(float duration)
    {
        movement.Stop(); 
        staggerDuration = duration;
        ChangeState(State.Stagger);
        HideTelegraph();
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