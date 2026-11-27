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

        Collider[] hits = Physics.OverlapBox(center, halfSize , owner.rotation, mask, QueryTriggerInteraction.Collide); // 보고 있는 방향으로 체크함

        foreach(Collider hit in hits)
        {
            AddTarget(result, hit);
        }

        return result;
    }

    public static List<IDamageable> CheckSector(Transform owner , float radius, float angle, LayerMask mask)
    {
        List<IDamageable> result = new List<IDamageable>();

        Collider[] hits = Physics.OverlapSphere(owner.position, radius, mask, QueryTriggerInteraction.Collide);
        foreach (Collider hit in hits)
        {
            Vector3 dir = hit.transform.position - owner.position;
            //dir.y = 0f;

            if(Vector3.Angle(owner.forward , dir) > angle / 2f)
                continue;

            AddTarget(result, hit);
        }

        return result;
    }

    // 투사체 구 판정용
    public static List<IDamageable> CheckSphere(Vector3 center, float radius, LayerMask mask)
    {
        List<IDamageable> result = new List<IDamageable>();

        Collider[] hits = Physics.OverlapSphere(center, radius, mask, QueryTriggerInteraction.Collide);
        foreach (Collider hit in hits)
        {
            AddTarget(result, hit);
        }

        return result;
    }

    // AOE용 체크
    public static List<IDamageable> CheckSectorAoe(Vector3 center, float radius, float angle, LayerMask mask)
    {
        List<IDamageable> result = new List<IDamageable>();

        Collider[] hits = Physics.OverlapSphere(center, radius, mask, QueryTriggerInteraction.Collide);
        foreach (Collider hit in hits)
        {
            Vector3 dir = hit.transform.position - center;
            //dir.y = 0f;

            if(Vector3.Angle(Vector3.forward , dir) > angle / 2f)
                continue;

            AddTarget(result, hit);
        }

        return result;
    }
    public static List<IDamageable> CheckBoxAoe(Vector3 center, float width, float length, LayerMask mask)
    {
        List<IDamageable> result = new List<IDamageable>();
        Vector3 halfSize = new Vector3(width / 2f, HitHieght / 2f , length / 2f);
        
        Collider[] hits = Physics.OverlapBox(center, halfSize , Quaternion.identity, mask, QueryTriggerInteraction.Collide); // 보고 있는 방향으로 체크함
        foreach(Collider hit in hits)
        {
            AddTarget(result, hit);
        }
        return result;
    }

    // 콜라이더가 자식 오브젝트(PlayerHurtbox 등)에 있어도 부모의 IDamageable을 찾음
    // 한 대상에 콜라이더가 여러 개여도 결과에는 한 번만 담김
    private static void AddTarget(List<IDamageable> result, Collider hit)
    {
        IDamageable target = hit.GetComponentInParent<IDamageable>();
        if (target != null && !result.Contains(target))
            result.Add(target);
    }
}