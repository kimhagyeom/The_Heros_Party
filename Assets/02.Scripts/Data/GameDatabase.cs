using System.Collections.Generic;
using UnityEngine;

// 원본 에셋 보관소: ID로 원본을 찾을 때 사용 (값은 절대 바꾸지 않음)
[CreateAssetMenu(fileName = "GameDatabase", menuName = "Data/GameDatabase")]
public class GameDatabase : ScriptableObject
{
    public SkillData[] skills;
    public WeaponData[] weapons;
    public UpgradeData[] upgrades;

    private Dictionary<int, SkillData> skillLookup;
    private Dictionary<int, WeaponData> weaponLookup;
    private Dictionary<int, UpgradeData> upgradeLookup;

    // 플레이 시작할 때마다 새로 만듦 (에디터에서 에셋 수정해도 반영되게)
    void OnEnable() => BuildLookups();

    public void BuildLookups()
    {
        skillLookup = new Dictionary<int, SkillData>();
        weaponLookup = new Dictionary<int, WeaponData>();
        upgradeLookup = new Dictionary<int, UpgradeData>();

        if (skills != null)
            foreach (var s in skills) if (s != null) skillLookup[s.skill_ID] = s;
        if (weapons != null)
            foreach (var w in weapons) if (w != null) weaponLookup[w.weapon_ID] = w;
        if (upgrades != null)
            foreach (var u in upgrades) if (u != null) upgradeLookup[u.upgrade_ID] = u;
    }

    public SkillData GetSkill(int id)   => id != 0 && skillLookup.TryGetValue(id, out var s) ? s : null;
    public WeaponData GetWeapon(int id) => id != 0 && weaponLookup.TryGetValue(id, out var w) ? w : null;

    public Dictionary<int, UpgradeData> UpgradeLookup => upgradeLookup;
}