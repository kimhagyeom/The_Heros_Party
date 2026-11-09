using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class TeleportPattern : BossPattern
{
    [Header("발동 조건")]
    [SerializeField] private float triggerDistance = 8f;     // 플레이어가 이보다 멀면 사용

    [Header("이동 위치")]
    [SerializeField] private float minDistance = 3f;         // 플레이어로부터 최소 거리 (바로 옆 방지)
    [SerializeField] private float maxDistance = 5f;         // 플레이어로부터 최대 거리
    [SerializeField] private int maxPickTries = 10;
    public Vector3 ? OverrideDestiantion {get; set;}

    [Header("연출")]
    [SerializeField] private GameObject bossModel;           // 보스의 보이는 부분 (비워두면 숨기기 생략)
    [SerializeField] private float vanishTime = 0.3f;        // 사라진 채로 있는 시간
    [SerializeField] private float appearDelay = 0.4f;       // 나타난 뒤 다음 행동까지 여유

    private NavMeshAgent agent;
    private Transform bossRoot;
    private Transform player;

    void Start()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        bossRoot = agent != null ? agent.transform : transform.root;

        var pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }

    // 쿨타임 + 플레이어가 멀리 있을 때만
    public override bool CanUse()
    {
        if (!base.CanUse()) return false;
        if (player == null || bossRoot == null) return false;

        Vector3 diff = player.position - bossRoot.position;
        diff.y = 0f;
        return diff.magnitude > triggerDistance;
    }

    protected override IEnumerator Execute(BossContext ctx)
    {
        Vector3 teleportPos = bossRoot.position;   // 끝까지 못 찾으면 제자리

        if (OverrideDestiantion.HasValue)
        {
            teleportPos = OverrideDestiantion.Value;
            OverrideDestiantion = null;
        }
        else
        {
            // 1. 기준점
            Vector3 basePos = ctx.target.position;
            if (NavMesh.SamplePosition(basePos, out NavMeshHit baseHit, 5f, NavMesh.AllAreas))
                basePos = baseHit.position;

            // 2. 플레이어 주변 고리에서 고르기
            for (int t = 0; t < maxPickTries; t++)
            {
                Vector2 dir = Random.insideUnitCircle.normalized;
                float dist = Random.Range(minDistance, maxDistance);
                Vector3 candidate = basePos + new Vector3(dir.x, 0f, dir.y) * dist;

                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1f, NavMesh.AllAreas))
                {
                    teleportPos = hit.position;
                    break;
                }
            }
        }
        Debug.Log($"[Teleport] {bossRoot.position} → {teleportPos}");

        // 3. 사라지기 (사라진 동안 무적)
        ctx.bossHealth.AddInvincible(vanishTime);
        SetVisible(false);
        yield return new WaitForSeconds(vanishTime);

        // 4. 이동 (NavMeshAgent가 있으면 반드시 Warp)
        if (agent != null) agent.Warp(teleportPos);
        else bossRoot.position = teleportPos;

        // 5. 나타나기 + 짧은 여유
        SetVisible(true);
        yield return new WaitForSeconds(appearDelay);
    }

    // 사라진 도중 끊기면 다시 보이게
    protected override void OnStopped()
    {
        SetVisible(true);
    }

    void SetVisible(bool visible)
    {
        if (bossModel != null) bossModel.SetActive(visible);
    }
}