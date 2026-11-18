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
            
            int targetSkillID = upgradeData.upgrade_ID / 10;   // 10002 → 1000
            if (slot.skill_ID == targetSkillID)
            {
                applySkillData = slot;
                break;
            }
        }

        switch (upgradeData.upgrade_Type)
        {
            case UpgradeType.Skill:
                UpgradeSkill(applySkillData, upgradeData.upgradeSkills);
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
    public void UpgradeSkill(SkillData applySkill, UpgradeSkills upgradeData)
    {
        if (applySkill == null || upgradeData == null) return;

        applySkill.dmg_Rate += upgradeData.dmg_Rate_Increment;
        applySkill.max_Stacks += upgradeData.projectile_Count_Increment;
        applySkill.cooldown -= upgradeData.cooldown_Increment;
        applySkill.windup_Time -= upgradeData.windup_Increment;
        applySkill.range += upgradeData.range_Increment;
    }
}
