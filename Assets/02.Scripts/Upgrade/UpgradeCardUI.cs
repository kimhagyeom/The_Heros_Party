using UnityEngine;

public class UpgradeCardUI : MonoBehaviour
{
    public UpgradeData upgradeData;
    public TMPro.TextMeshProUGUI titleText;
    public TMPro.TextMeshProUGUI descriptionText;
    public TMPro.TextMeshProUGUI amountText;

    public void SetUpgrade()
    {
        UpgradeManager.Instance.UpgradeSkill(upgradeData);
        UpgradeSelector.Instance.upgradePanel.SetActive(false);
    }
    public void SetData(UpgradeData data)
    {
        upgradeData = data;
        titleText.text = data.upgrade_Name;
        descriptionText.text = GetDescription(data);
    }
    private string GetDescription(UpgradeData data)
    {
        switch (data.upgrade_Type)
        {
            case UpgradeType.Skill:
                return GetSkillDescription(data.upgradeSkills);
            case UpgradeType.Status:
                return GetStatusDescription(data.status_Increment);
        }
        return "";
    }
    private string GetSkillDescription(UpgradeSkills skill)
    {
        if (skill == null) return "";

        string description = "";

        if (skill.dmg_Rate_Increment > 0)
            description += $"Damage +{skill.dmg_Rate_Increment * 100}%\n";
        if (skill.projectile_Count_Increment > 0)
            description += $"Projectile +{skill.projectile_Count_Increment}\n";
        if (skill.cooldown_Increment > 0)
            description += $"Cooldown -{skill.cooldown_Increment}s\n";
        if (skill.windup_Increment > 0)
            description += $"Windup -{skill.windup_Increment}s\n";
        if (skill.range_Increment > 0)
            description += $"Range +{skill.range_Increment}m\n";

        return description;
    }

    private string GetStatusDescription(UpgradeStatus status)
    {
        if(status == null) return "";

        string description = "";

        if (status.increase_Speed > 0)
            description += $"Speed +{status.increase_Speed}\n";
        if (status.increase_maxStamina > 0)
            description += $"Max Stamina +{status.increase_maxStamina}\n";
        if (status.increase_BaseAtk > 0)
            description += $"Base Attack +{status.increase_BaseAtk}\n";

        return description;
    }
}
