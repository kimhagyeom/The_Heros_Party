using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class MeteorPattern : BossPattern
{
    [SerializeField] private GameObject meteorPrefab;
    [SerializeField] private Vector3 spawnHight;
    [SerializeField] private float fallSpeed;
    readonly private AoeAttack aoeAttack = new AoeAttack();

    MeteorData D => (MeteorData)data;
    protected override IEnumerator Execute(BossContext ctx)
    {
        Vector3 targetPos = ctx.target.position + spawnHight;
        GameObject meteor = Instantiate(meteorPrefab, targetPos, Quaternion.identity);

        float groundY = targetPos.y; 

        while(meteor.transform.position.y > targetPos.y)
        {
            meteor.transform.position -= new Vector3(0,1,0) * Time.deltaTime * fallSpeed;
            yield return null;
        }
        targetPos.y = groundY; 
        meteor.transform.position = targetPos;

        HitboxInfo hitboxInfo = new HitboxInfo
        {
             shape = HitboxShape.Sector, 
             atk_Type = AttackType.Aoe,
             atk_Range = 10f, 
             radius = D.impactRadius, 
             angle = 360f 
        };

        DamageInfo dmg = new DamageInfo { damage = D.damage };
        yield return new WaitForSeconds(D.warningTime);
        aoeAttack.Execute(targetPos,hitboxInfo, dmg ,ctx.targetMask);
        yield return null;
    }
}
