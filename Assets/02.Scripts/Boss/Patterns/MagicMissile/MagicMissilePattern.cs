using System.Collections;
using UnityEngine;

public class MagicMissilePattern : BossPattern
{
    [SerializeField] private Projectile missilePrefab;
    readonly ProjectileAttack projectileAttack = new ProjectileAttack();
    MagicMissileData D => (MagicMissileData)data;

    [SerializeField] private float sideOffset = 0.5f;      // 좌우 미사일 간격
    [SerializeField] private float infection = 0f;         // 정신 감염도
    [SerializeField] private float telegraphWidth = 0.3f;  // 경고선 두께
    [SerializeField] private float spawnHeight = 1f;       // 발사 높이

    protected override IEnumerator Execute(BossContext ctx)
    {
        //조준 및 예고,발사 전까지 플레이어 방향으로 경고선 유지
        Vector3 dir = transform.forward;
        float t = 0f;
        while (t < D.warningTime)
        {
            dir = GetDirToTarget(ctx);
            ctx.telegraph.ShowBoxAt(transform.position, Quaternion.LookRotation(dir), telegraphWidth, D.Missile_Range);
            t += Time.deltaTime;
            yield return null;
        }
        ctx.telegraph.Hide();

        //모두 같은 방향으로 직선 이동
        Vector3 right = Vector3.Cross(Vector3.up, dir);
        Quaternion rot = Quaternion.LookRotation(dir);

        var settings = new ProjectileSettings
        {
            speed = D.Missile_Speed,
            lifeTime = D.Missile_Speed > 0f ? D.Missile_Range / D.Missile_Speed : 0f,
            radius = D.Missile_Radius,
            pierce = false,
            targetMask = ctx.targetMask,
            obstacleMask = 0
        };
        var dmg = new DamageInfo { damage = D.damage, infection = infection };

        for (int i = 0; i < D.Missile_Count; i++)
        {
            int side = (i % 2 == 1) ? -(i + 1) / 2 : i / 2;
            Vector3 pos = transform.position + Vector3.up * spawnHeight + right * side * sideOffset;

            Projectile p = projectileAttack.Shoot(missilePrefab, pos, rot, settings, dmg);
            Track(p.gameObject);

            if (i < D.Missile_Count - 1)
                yield return new WaitForSeconds(D.Missile_Interval);
        }
    }

    Vector3 GetDirToTarget(BossContext ctx)
    {
        Vector3 d = ctx.target.position - transform.position;
        d.y = 0f;
        return d.sqrMagnitude > 0.0001f ? d.normalized : transform.forward;
    }
}