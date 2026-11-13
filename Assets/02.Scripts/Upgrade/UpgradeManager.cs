using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager>
{
    [SerializeField] private PlayerSkillController playerSkillController;
    [SerializeField] private PlayerController playerController;

    public void UpgradeSkill(UpgradeData upgradeData)
    {
        if (upgradeData == null) return;

        SkillData applySkillData = null;

        foreach(var slot in playerSkillController.slotSkills)
        {
            if (slot == null) continue;

            if (slot.skill_ID == upgradeData.upgrade_ID)
            {
                applySkillData = slot;
                break;
            }
        }

        switch (upgradeData.upgrade_Type)
        {
            case UpgradeType.Damage:
                applySkillData.dmg_Rate += upgradeData.dmg_Rate_Increment;
                break;
            case UpgradeType.Cooldown:
                applySkillData.cooldown -= upgradeData.dmg_Rate_Increment;
                break;
            case UpgradeType.Stacks:
                applySkillData.max_Stacks += upgradeData.projectile_Count_Increment;
                break;
            case UpgradeType.Windup:
                applySkillData.windup_Time -= upgradeData.dmg_Rate_Increment;
                break;
            case UpgradeType.Range:
                applySkillData.range += upgradeData.dmg_Rate_Increment;
                break;
            case UpgradeType.Status:
                UpgradePlayer(upgradeData.status_Increment);
                break;
        }

        //skillData.Upgrade();
    }
    public void UpgradePlayer(UpgradeStatus upgradeData)
    {
        if(upgradeData == null) return;
        
        playerController.stats.Move_Speed += upgradeData.increase_Speed;
        playerController.stats.maxStamina += upgradeData.increase_maxStamina;
        playerController.stats.base_Atk += upgradeData.increase_BaseAtk;
    }
}
