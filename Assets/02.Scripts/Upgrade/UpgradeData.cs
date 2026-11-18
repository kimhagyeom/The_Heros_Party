using UnityEngine;

public enum UpgradeType { Skill, Status }

[System.Serializable]
public class UpgradeStatus
{
    public float increase_Speed;
    public float increase_maxStamina;
    public float increase_BaseAtk;
}
[System.Serializable]
public class UpgradeSkills
{
    public float dmg_Rate_Increment;
    public int projectile_Count_Increment;
    public float cooldown_Increment;
    public float windup_Increment;
    public float range_Increment;
}
[CreateAssetMenu(fileName = "UpgradeData", menuName = "Upgrade/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public SkillData skillData;
    public int upgrade_ID;
    public string upgrade_Name;
    public UpgradeType upgrade_Type;
    public UpgradeSkills upgradeSkills;
    public UpgradeStatus status_Increment;
}
