using UnityEngine;

[CreateAssetMenu(menuName = "Boss/Patterns/ShieldData")]
public class ShieldData : BossPatternData
{
    public float trigger_HP1;
    public float trigger_HP2;
    public int activation_Count;
    public float shield_Duration;
    public bool End_On_Break;
    public float Break_Stun_Duration;
    public float Vulnerable_Multiplier;
    public float Fail_Invincible_Duration;
}
