using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager>
{
    [SerializeField] private PlayerSkillController playerSkillController;
    [SerializeField] private PlayerController playerController;
    private Dictionary<int, UpgradeProgress> progressMap = new Dictionary<int, UpgradeProgress>();

    public UpgradeProgress GetProgress(int targetId)
    {
        if (!progressMap.TryGetValue(targetId, out var p))
        {
            p = new UpgradeProgress();
            progressMap[targetId] = p;
        }
        return p;
    }

    public void UpgradeSkill(UpgradeData upgradeData)
    {
        if (upgradeData == null) return;

        
        int targetSkillID = upgradeData.upgrade_ID / 10;

        SkillData applySkillData = null;
        UpgradeProgress progress = GetProgress(targetSkillID);

        foreach(var slot in playerSkillController.slotSkills)
        {
            if (slot == null) continue;
            if (slot.skill_ID == targetSkillID) // 10002 → 1000
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

        
        if (upgradeData.upgrade_Grade == UpgradeGrade.Special)
        {
            progress.takenSpecials.Add(upgradeData.upgrade_ID);
            return;
        }
        upgradeData.upgradeProgress.normalCount++;

        if (progress.normalCount % 3 == 0 && !progress.IsComplete)
        {
            Debug.Log($"[{targetSkillID}] 특수 강화 선택 타이밍 ({progress.normalCount}회)");
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
