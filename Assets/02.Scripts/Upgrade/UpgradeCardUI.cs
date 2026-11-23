using UnityEngine;

public class UpgradeCardUI : MonoBehaviour
{
    public UpgradeData upgradeData;
    public TMPro.TextMeshProUGUI titleText;
    public TMPro.TextMeshProUGUI descriptionText;
    public TMPro.TextMeshProUGUI amountText;
    public TMPro.TextMeshProUGUI smallNameText; // 작은 이름 (검, 회전베기)

    public void SetUpgrade()
    {
        UpgradeSelector.Instance.Pick(upgradeData);
    }
    public void SetData(UpgradeData data)
    {
        upgradeData = data;
        titleText.text = data.display_Name;
        descriptionText.text = data.display_Description;
        smallNameText.text = data.display_Small_Name;
        amountText.text = GetProgressText(data);
    }
    // 이 카드 고르면 몇 번째인지
    private string GetProgressText(UpgradeData data)
    {
        UpgradeProgress p = UpgradeManager.Instance.GetProgress(data.target_ID);

        if (data.upgrade_Grade == UpgradeGrade.Special)
            return $"특수 {p.takenSpecials.Count + 1}/3";

        int count = UpgradeManager.Instance.specialTriggerCount;
        int next = p.normalCount + 1;
        int left = count - next % count; // 특수까지 남은 횟수
        if (left == count)
            return $"일반 {next}회 → 특수 강화!";
        return $"일반 {next}회 (특수까지 {left}회)";
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
