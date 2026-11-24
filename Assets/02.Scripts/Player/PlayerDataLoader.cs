using UnityEngine;
using UnityEngine.SceneManagement;

// 영구 데이터 불러와서 Player에 적용 / 베이스캠프에서 저장
public class PlayerDataLoader : MonoBehaviour
{
    [SerializeField] private DataTable dataTable;

    private Player player;
    private PlayerController controller;
    private PlayerAttack attack;
    private PlayerSkillController skill;

    void Awake()
    {
        player = GetComponent<Player>();
        controller = GetComponent<PlayerController>();
        attack = GetComponent<PlayerAttack>();
        skill = GetComponent<PlayerSkillController>();
    }

    void Start()
    {
        PermanentData data = SaveManager.LoadPermanent();

        bool isDungeon = SceneManager.GetActiveScene().name != "BaseCamp";

        // 1. 스탯은 어디서든 먼저
        if (data != null)
            LoadStats(data);

        if (isDungeon)
        {
            // 2. 던전: 장비·강화·HP는 RunSaveManager가 (이어하기 or 새 런)
            RunSaveManager.Instance.StartRun();
        }
        else
        {
            // 2. 베이스캠프: 장비 불러오기
            if (data != null)
                LoadEquipment(data);
        }
    }

    void LoadStats(PermanentData data)
    {
        controller.stats.maxHealth = data.maxHealth;
        controller.stats.Move_Speed = data.moveSpeed;
        controller.stats.base_Atk = data.baseAtk;
        controller.stats.maxStamina = data.maxStamina;

        player.RefreshStats();
    }

    void LoadEquipment(PermanentData data)
    {
        attack.SetMainWeapon(dataTable.FindWeapon(data.mainWeaponID));
        attack.SetSubWeapon(dataTable.FindWeapon(data.subWeaponID));

        for (int i = 0; i < data.skillIDs.Length; i++)
        {
            skill.SetSkill(i, dataTable.FindSkill(data.skillIDs[i]));
        }
    }

    public void Save()
    {
        PermanentData data = new PermanentData();

        data.maxHealth = controller.stats.maxHealth;
        data.moveSpeed = controller.stats.Move_Speed;
        data.baseAtk = controller.stats.base_Atk;
        data.maxStamina = controller.stats.maxStamina;

        data.mainWeaponID = GetWeaponID(attack.mainWeaponData);
        data.subWeaponID = GetWeaponID(attack.subWeaponData);

        data.skillIDs = new int[PlayerSkillController.SlotCount];
        for (int i = 0; i < data.skillIDs.Length; i++)
        {
            data.skillIDs[i] = GetSkillID(skill.slotSkills[i]);
        }

        SaveManager.SavePermanent(data);
       // Debug.Log("[저장] 영구 데이터 저장");
    }

    int GetWeaponID(WeaponData weapon)
    {
        if (weapon == null) return 0;
        return weapon.weapon_ID;
    }

    int GetSkillID(SkillData skillData)
    {
        if (skillData == null) return 0;
        return skillData.skill_ID;
    }
}
