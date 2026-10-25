using UnityEngine;

// 모든 패턴이 공통으로 갖는 값
public class BossPatternData : ScriptableObject
{
    public int patternId;
    public float castTime;
    public float cooldown;
    public float warningTime;
    public int damage;
}

// 패턴별 값은 상속해서 추가
[CreateAssetMenu(menuName = "Boss/IceSpikeData")]
public class IceSpikeData : BossPatternData
{
    public int Spike_Set_Count;
    public float Spike_interval;
    public float S_Spike_Radius;
    public float M_Spike_Radius;
    public float L_Spike_Radius;
    public int Shard_Count;
    public int Shard_Speed; //float?
}

[CreateAssetMenu(menuName = "Boss/MeteorData")]
public class MeteorData : BossPatternData
{
    public int meteorCount;
    public float impactRadius;
    public float Burn_Ground_Duration;
}

