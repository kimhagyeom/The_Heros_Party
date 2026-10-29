using UnityEngine;

[CreateAssetMenu(menuName = "Boss/Patterns/MagicMissileData")]
public class MagicMissileData : BossPatternData
{
    public int Missile_Count;
    public float Missile_Speed;
    public float Missile_Interval;
    public float Missile_Range;
    public float Missile_Radius;
    public bool Homing_Enabled;
    public float Homing_Strength;
}