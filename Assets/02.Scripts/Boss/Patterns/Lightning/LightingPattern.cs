using System.Collections;
using System.Data.Common;
using UnityEngine;
using UnityEngine.AI; 

public class LightingPattern : BossPattern
{
    [SerializeField] private GameObject lightingPrefab;
    readonly private AoeAttack aoeAttack = new AoeAttack();
    LightingData D => (LightingData)data;

    [Header("시트에 없는 값 (임시, 기획 확인)")]
    [SerializeField] private float random_Atk_Radius = 10f;
    [SerializeField] private float lighting_duration = 0.2f;
    [SerializeField] private int maxPickTries = 10; 

    protected override IEnumerator Execute(BossContext ctx)
    {
        // 1. 플레이어 위치를 바닥에 붙여서 기준점으로 (점프·낙하 중이어도 바닥 높이)
        Vector3 basePos = ctx.target.position;
        if (NavMesh.SamplePosition(basePos, out NavMeshHit baseHit, 5f, NavMesh.AllAreas))
            basePos = baseHit.position;

        // 첫 번째 낙뢰: 플레이어 위치(바닥)
        yield return LightingAttack(basePos, ctx);

        for (int i = 0; i < D.random_Strike_Count; i++)
        {
            Vector3 strikePos = basePos;   // 끝까지 못 찾으면 기준점에 떨어짐

            for (int t = 0; t < maxPickTries; t++)
            {
                // 2. 기준점 + 랜덤 오프셋
                Vector2 r = Random.insideUnitCircle * random_Atk_Radius;
                Vector3 randomAtkPos = basePos + new Vector3(r.x, 0f, r.y);

                // 3. 맵 안인지 검사 (거리 작게 → 맵 밖이면 false)
                if (NavMesh.SamplePosition(randomAtkPos, out NavMeshHit hit, 1f, NavMesh.AllAreas))
                {
                    strikePos = hit.position;
                    break;
                }
            }

            yield return LightingAttack(strikePos, ctx);
        }
    }
    
    IEnumerator LightingAttack(Vector3 targetPos, BossContext ctx)
    {
        HitboxInfo hitboxInfo = new HitboxInfo
        {
             shape = HitboxShape.Sector, 
             atk_Type = AttackType.Aoe,
             atk_Range = 10f, 
             radius = D.atk_Radius, 
             angle = 360f 
        };

        ctx.telegraph.Show(hitboxInfo, targetPos);
        yield return new WaitForSeconds(D.warningTime);
        ctx.telegraph.Hide();

        GameObject lighting = Instantiate(lightingPrefab,targetPos,Quaternion.identity);
        Destroy(lighting, lighting_duration);
        DamageInfo dmg = new DamageInfo { damage = D.damage };
        aoeAttack.Execute(targetPos,hitboxInfo, dmg ,ctx.targetMask);
    }
}
