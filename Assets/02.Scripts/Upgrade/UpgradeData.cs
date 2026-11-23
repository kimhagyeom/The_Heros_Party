using System.Collections.Generic;
using UnityEngine;

public enum UpgradeType { Skill, Status }
public enum UpgradeGrade { Normal, Special }
public enum UpgradeTargetType { Normal_Attack, Skill }

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
    public float dmg_Rate_Increment; // 공격력 +%
    public int projectile_Count_Increment;  // 투사체 +개
    public float cooldown_Increment; // 쿨타임 -%
    public float windup_Increment;
    public float range_Increment; // 범위 / 사거리 +%
    public float attack_Speed_Increment; // 공격속도 +%
    public int max_Stacks_Increment;// 충전량 +개
}
[System.Serializable]
public class UpgradeProgress
{
    public int normalCount; //일반강화 횟수
    public List<int> takenSpecials = new List<int>(); //얻은 특수 강화 ID 저장
    public bool IsComplete => takenSpecials.Count >= 3; //특수 3개면 완료
}

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Upgrade/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public SkillData skillData;
    public int upgrade_ID;
    public string upgrade_Name;
    public UpgradeType upgrade_Type;
    public UpgradeGrade upgrade_Grade;
    public UpgradeSkills upgradeSkills;
    public UpgradeStatus status_Increment;
    public UpgradeTargetType target_Type;
    public int target_ID;
    public bool is_Enabled = true;
    public string display_Name;
    public string display_Small_Name;
    [TextArea] public string display_Description;

}
