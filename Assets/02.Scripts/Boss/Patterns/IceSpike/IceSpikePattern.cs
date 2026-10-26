using System.Collections;
using UnityEngine;

public class IceSpikePattern : BossPattern
{
    [SerializeField] private GameObject iceSpike;
    [SerializeField] private GameObject iceShard;
    [SerializeField] private AttackTelegraph telegraph;
    [SerializeField] private AoeAttack aoeAttack;
    [SerializeField] private ProjectileAttack projectileAttack;

    IceSpikeData D => (IceSpikeData)data;
    protected override IEnumerator Execute(BossContext ctx)
    {
        float[] radii = {D.S_Spike_Radius, D.M_Spike_Radius, D.L_Spike_Radius};
        GameObject lastSpike = null;

        for(int set = 0; set < D.Spike_Set_Count; set++)
        {
            

            foreach(float r in radii)
            {
                Vector3 targetPos = ctx.target.position;
                TelegraphInfo tInfo = new TelegraphInfo
                {
                    shape = HitboxShape.Sector,
                    radius = r,
                    angle = 360f,
                    duration = D.warningTime
                };
                // telegraph.Show(tInfo,targetPos,Vector3.forward);  매개변수 수정에정

                yield return new WaitForSeconds(D.castTime);
                GameObject spike = Instantiate(iceSpike , targetPos, Quaternion.identity);
                spike.transform.localScale = Vector3.one * (r * 2f); //프리팹 지름 1기준
                if(set < D.Spike_Set_Count) Destroy(spike , 2f); // 얼음 가시 유지 시간
                else lastSpike = spike;
            }
        }
        yield return new WaitForSeconds(2f); // 얼음 가시 생존 시간

        Vector3 lastPos = lastSpike.transform.position;
        Destroy(lastSpike);
        SpawnShard(lastPos);
    }
    private void SpawnShard(Vector3 pos)
    {
        float step = 360f / D.Shard_Count;

        for(int i = 0; i < D.Shard_Count; i++)
        {
            float angle = step * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
        
            //projectileAttack.Shoot(pos,D.Shard_Speed); // 분리 예정
        }
    }
}
