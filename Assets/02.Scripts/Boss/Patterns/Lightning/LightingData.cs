using UnityEngine;

[CreateAssetMenu(menuName = "Boss/Patterns/LightingData")]
public class LightingData : BossPatternData
{
    public int lighting_Count;
    public int set_Count;
    public int target_Strike_Count;
    public int random_Strike_Count;
    public float atk_Interval;
    public float atk_Radius;
    public float Burn_Ground_Duration;
}
