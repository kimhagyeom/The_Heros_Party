using UnityEngine;

// 전체 무기/스킬 목록 (저장된 ID로 에셋 찾을 때 사용)
[CreateAssetMenu(menuName = "DataTable")]
public class DataTable : ScriptableObject
{
    public WeaponData[] weapons;
    public SkillData[] skills;

    public WeaponData FindWeapon(int id)
    {
        foreach (WeaponData weapon in weapons)
        {
            if (weapon.weapon_ID == id) return weapon;
        }
        return null;
    }

    public SkillData FindSkill(int id)
    {
        foreach (SkillData skill in skills)
        {
            if (skill.skill_ID == id) return skill;
        }
        return null;
    }
}
