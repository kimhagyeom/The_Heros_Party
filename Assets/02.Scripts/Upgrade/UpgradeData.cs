using UnityEngine;

public enum UpgradeType { Damage, Cooldown, Stacks, Windup, Range, Status }

[System.Serializable]
public class UpgradeStatus
{
    public float increase_Speed;
    public float increase_maxStamina;
    public float increase_BaseAtk;
}
[CreateAssetMenu(fileName = "UpgradeData", menuName = "Upgrade/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public int upgrade_ID;
    public string upgrade_Name;
    public UpgradeType upgrade_Type;
    public float dmg_Rate_Increment;
    public int projectile_Count_Increment;
    public UpgradeStatus status_Increment;
}
