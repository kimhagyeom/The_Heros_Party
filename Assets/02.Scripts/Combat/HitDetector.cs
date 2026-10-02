using System.Collections.Generic;
using UnityEngine;

public class HitDetector
{
    private const float HitHieght = 2f;
    public static List<IDamageable> CheckBox(Transform owner, float width, float length, LayerMask mask)
    {
        

        List<IDamageable> result = new List<IDamageable>();

        Vector3 center = owner.position + owner.forward * length/2;
        Vector3 halfSize = new Vector3(width / 2f, HitHieght / 2f , length / 2f);

        Collider[] hits = Physics.OverlapBox(center, halfSize , owner.rotation, mask); // 보고 있는 방향으로 체크함

        foreach(Collider hit in hits)
        {
            if(hit.TryGetComponent(out IDamageable target))
                result.Add(target);
        }

        return result;
    }

    public static List<IDamageable> CheckSector(Transform owner , float radius, float angle, LayerMask mask)
    {
        List<IDamageable> result = new List<IDamageable>();

        Collider[] hits = Physics.OverlapSphere(owner.position, radius, mask);
        foreach (Collider hit in hits)
        {
            Vector3 dir = hit.transform.position - owner.position;
            dir.y = 0f;

            if(Vector3.Angle(owner.forward , dir) > angle / 2f)
                continue;

            if (hit.TryGetComponent(out IDamageable target))
                result.Add(target);
        }

        return result;
    }
}
