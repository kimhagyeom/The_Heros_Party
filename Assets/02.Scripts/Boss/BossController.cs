using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PhaseInfo
{
    public float hpRatio;              // 이 페이즈가 시작되는 체력 비율
    public List<BossPattern> patterns; // 이 페이즈에서 쓰는 패턴
}
public class BossController : MonoBehaviour
{
    [SerializeField] List<PhaseInfo> phases;
    [SerializeField] float idleTime;
}
