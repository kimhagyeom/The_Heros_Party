using System.Collections;
using UnityEngine;

public class IceSpikePattern : BossPattern
{
    [SerializeField] private GameObject iceSpikePrefab;
    [SerializeField] private Projectile iceShardPrefab;
    readonly AoeAttack aoeAttack = new AoeAttack();
    readonly ProjectileAttack projectileAttack = new ProjectileAttack();
    
    IceSpikeData D => (IceSpikeData)data;

    [Header("시트에 없는 값 (임시, 기획 확인)")]
    [SerializeField] private float spikeLifeTime = 1.4f;
    [SerializeField] private float shardLifeTime = 10f;
     [SerializeField] float shardRadius = 0.3f; // 파편 감지 범위
    [SerializeField] bool shardPierce = false; //파편 관통 여부


    protected override IEnumerator Execute(BossContext ctx)
    {
        float[] radii = {D.S_Spike_Radius, D.M_Spike_Radius, D.L_Spike_Radius};
        GameObject lastSpike = null;
        Vector3 lastPos = Vector3.zero;

        Vector3 targetPos = ctx.target.position;
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;
        Vector3 atkDir = dir.normalized;

        Vector3 atkPos = transform.position;

        //for(int set = 0; set < D.Spike_Set_Count; set++) // SetCount 필요 없을듯?
        //{
            for (int i = 0; i < radii.Length; i++)
            {
                float r = radii[i];
                // bool isLast = set == D.Spike_Set_Count - 1 && i == radii.Length - 1; //여기도 어차피 작중큰 얼음가시 소환하면 되니까
                
                if (i == 0) atkPos += atkDir * radii[i];
                else atkPos += atkDir * (radii[i] * 2f - radii[i - 1]);
                    

                var hitbox = new HitboxInfo { shape = HitboxShape.Sector, radius = r, angle = 360f };
                //ctx.telegraph.Show(hitbox, ctx.target.position);   추후 AttackTelegraph 수정 예정

                yield return new WaitForSeconds(D.warningTime);
                GameObject spike = Instantiate(iceSpikePrefab , atkPos, Quaternion.identity);
                spike.transform.localScale = Vector3.one * (r * 2f); //프리팹 지름 1기준
                DamageInfo dmg = new DamageInfo { damage = D.damage };
                aoeAttack.Execute(atkPos, hitbox , dmg, ctx.targetMask);
                Debug.Log("생성 위치 : " + atkPos);
                if (i == radii.Length - 1)
                {
                    lastSpike = spike; 
                    lastPos = lastSpike.transform.position;
                    Debug.Log("Last Spike");
                }
                else
                {
                    Destroy(spike, spikeLifeTime);
                    yield return new WaitForSeconds(D.Spike_interval);
                }
            }
        //}
        yield return new WaitForSeconds(spikeLifeTime); // 얼음 가시 생존 시간

        if (lastSpike != null) Destroy(lastSpike);
        SpawnShard(lastPos, ctx);
    }
    private void SpawnShard(Vector3 lastPos , BossContext ctx)
    {
        Debug.Log("SpawnShard");
        float step = 360f / D.Shard_Count;

        var settings = new ProjectileSettings
        {
            speed = D.Shard_Speed,
            lifeTime = shardLifeTime,
            radius = shardRadius,
            pierce = shardPierce,
            targetMask = ctx.targetMask,
            obstacleMask = 0
        };
        var dmg = new DamageInfo { damage = D.damage };

        for(int i = 0; i < D.Shard_Count; i++)
        {
            float angle = step * i;
            Quaternion rot = Quaternion.Euler(0, angle, 0);
        
            Projectile p = projectileAttack.Shoot(iceShardPrefab, lastPos, rot, settings, dmg); 
        }
    }
}
