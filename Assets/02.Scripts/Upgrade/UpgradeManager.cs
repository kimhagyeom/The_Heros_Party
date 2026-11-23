using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager>
{
    [SerializeField] private PlayerSkillController playerSkillController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerAttack playerAttack;
    public int specialTriggerCount = 3; // 특수 강화 주기
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

        UpgradeProgress progress = GetProgress(upgradeData.target_ID);

        switch (upgradeData.upgrade_Type)
        {
            case UpgradeType.Skill:
                if (upgradeData.target_Type == UpgradeTargetType.Normal_Attack)
                    UpgradeAttack(upgradeData.upgradeSkills);
                else
                    UpgradeSkill(GetSkill(upgradeData.target_ID), upgradeData.upgradeSkills);
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
        progress.normalCount++;

        //skillData.Upgrade();
    }

    // 특수 강화 타이밍인지
    public bool NeedSpecial(int targetId)
    {
        UpgradeProgress p = GetProgress(targetId);
        return p.normalCount / specialTriggerCount > p.takenSpecials.Count;
    }

    // 장착중인 무기/스킬인지
    public bool IsEquipped(UpgradeData upgradeData)
    {
        if (upgradeData.target_Type == UpgradeTargetType.Normal_Attack)
            return playerAttack.mainWeaponData != null && playerAttack.mainWeaponData.weapon_ID == upgradeData.target_ID;

        return GetSkill(upgradeData.target_ID) != null;
    }

    // 장착한 스킬 찾기
    SkillData GetSkill(int skillId)
    {
        for (int i = 0; i < PlayerSkillController.SlotCount; i++)
        {
            SkillSlot slot = playerSkillController.GetSlot(i);
            if (slot != null && !slot.IsEmpty && slot.data.skill_ID == skillId)
                return slot.data;
        }
        return null;
    }

    public void UpgradeAttack(UpgradeSkills upgradeData)
    {
        if (upgradeData == null) return;

        playerAttack.upgradeDamageRate += upgradeData.dmg_Rate_Increment;
        playerAttack.upgradeRangeRate += upgradeData.range_Increment;
        playerAttack.upgradeSpeedRate += upgradeData.attack_Speed_Increment;
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

        // 0.1 = 10%
        applySkill.dmg_Rate *= 1f + upgradeData.dmg_Rate_Increment;
        applySkill.projectile_Count += upgradeData.projectile_Count_Increment;
        applySkill.max_Stacks += upgradeData.max_Stacks_Increment;
        applySkill.cooldown *= 1f - upgradeData.cooldown_Increment;
        applySkill.windup_Time -= upgradeData.windup_Increment;
        applySkill.range *= 1f + upgradeData.range_Increment; // 부채꼴
        applySkill.length *= 1f + upgradeData.range_Increment; // 박스
    }
}
