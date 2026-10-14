using System;
using System.Collections.Generic;
using UnityEngine;

// 투사체 설정: 쏘는 쪽(적 ProjectileAttack / 플레이어 SkillExecutor)이 자기 데이터에서 풀어서 넘김
public struct ProjectileSettings
{
    public float speed;
    public float lifeTime;          // 0 이하면 프리팹 기본값
    public float radius;            // 구 판정 반지름 (length가 0일 때 사용, 0 이하면 프리팹 기본값)
    public float width;             // 박스 판정 폭
    public float length;            // 박스 판정 길이 (0이면 구 판정)
    public bool pierce;             // 관통: 맞아도 사라지지 않음
    public LayerMask targetMask;
    public LayerMask obstacleMask;  // 닿으면 사라지는 벽 (0이면 벽 무시)
}

// 적/플레이어 공용 투사체: 날아가다가 맞은 대상을 onHit으로 알려줌 (데미지 계산은 쏜 쪽이 담당)
public class Projectile : MonoBehaviour
{
    [SerializeField] private float hitRadius;
    [SerializeField] private float lifeTime = 15f;

    private ProjectileSettings settings;
    private Action<IDamageable> onHit;
    private HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    private bool IsBox => settings.length > 0f;

    public void Init(ProjectileSettings s, Action<IDamageable> onHit)
    {
        settings = s;
        this.onHit = onHit;

        if (s.radius > 0f) hitRadius = s.radius;
        if (s.lifeTime > 0f) lifeTime = s.lifeTime;
    }

    void Update()
    {
        if (CheckHit()) return;

        float step = settings.speed * Time.deltaTime;
        if (Physics.Raycast(transform.position, transform.forward, step, settings.obstacleMask, QueryTriggerInteraction.Ignore))
        {
            Destroy(gameObject);
            return;
        }
        transform.position += transform.forward * step;

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
            Destroy(gameObject);
    }

    bool CheckHit()
    {
        List<IDamageable> targets = IsBox
            ? HitDetector.CheckBox(transform, settings.width, settings.length, settings.targetMask)
            : HitDetector.CheckSphere(transform.position, hitRadius, settings.targetMask);

        foreach (IDamageable target in targets)
        {
            if (!hitTargets.Add(target)) continue;   // 이미 맞은 대상이면 건너뜀
            onHit?.Invoke(target);

            if (!settings.pierce)
            {
                Destroy(gameObject);
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (IsBox)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position + transform.forward * settings.length / 2f, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(settings.width, 2f, settings.length));
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, hitRadius);
        }
    }
}
