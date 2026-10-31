using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class MeteorPattern : BossPattern
{
    [SerializeField] private GameObject meteorPrefab;
    [SerializeField] private BurnGround burnGroundPrefab;
    [SerializeField] private Vector3 spawnHight;
    [SerializeField] private float fallSpeed;
    readonly private AoeAttack aoeAttack = new AoeAttack();

    MeteorData D => (MeteorData)data;
    protected override IEnumerator Execute(BossContext ctx)
    {
        Vector3 targetPos = ctx.target.position;
        Vector3 spawnPos = targetPos + spawnHight;
        GameObject meteor = Instantiate(meteorPrefab, spawnPos, Quaternion.identity);

        float groundY = 0f; 
        float elapsed = 0f;
        
        HitboxInfo hitboxInfo = new HitboxInfo
        {
             shape = HitboxShape.Sector, 
             atk_Type = AttackType.Aoe,
             atk_Range = 10f, 
             radius = D.impactRadius, 
             angle = 360f 
        };
        ctx.telegraph.Show(hitboxInfo, targetPos);

        while(elapsed < D.castTime)
        {
            elapsed += Time.deltaTime;
            meteor.transform.position -= new Vector3(0,1,0) * Time.deltaTime * fallSpeed;
            groundY = meteor.transform.position.y;
            ctx.telegraph.SetProgress(elapsed/D.castTime);
            yield return null;
        }
        spawnPos.y = groundY; 
        meteor.transform.position = spawnPos;

        

        DamageInfo dmg = new DamageInfo { damage = D.damage };
        ctx.telegraph.Hide();
        //yield return new WaitForSeconds(D.castTime);
        //Debug.Log(spawnPos);
        aoeAttack.Execute(spawnPos,hitboxInfo, dmg ,ctx.targetMask);

        Destroy(meteor);

        BurnGroundSettings bgSettings = new BurnGroundSettings
        {
            radius = D.impactRadius * 2f,
            lifeTime = D.Burn_Ground_Duration,
            damage = D.damage,
            targetMask = ctx.targetMask
        };
        BurnGround burnGround = Instantiate(burnGroundPrefab, targetPos, Quaternion.identity);
        burnGround.Init(bgSettings);
        yield return null;
    }
}
