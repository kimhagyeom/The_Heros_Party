using UnityEngine;

[CreateAssetMenu(menuName = "Boss/Patterns/IceSpikeData")]
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