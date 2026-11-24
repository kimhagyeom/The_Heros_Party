using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class RunSaveManager : Singleton<RunSaveManager>
{
    [SerializeField] private GameDatabase database;
    [SerializeField] private PlayerSkillController skillController;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private Health playerHealth;

    [Header("meta_save 없을 때 기본 장비 (첫 실행)")]
    [SerializeField] private int defaultMainWeaponId = 1001;
    [SerializeField] private int defaultSubWeaponId = 1002;
    [SerializeField] private int[] defaultSkillIds = { 10001, 10002, 0, 10003 };

    string SavePath => Path.Combine(Application.persistentDataPath, "run_save.json");

    public bool HasSave() => File.Exists(SavePath);

    // ── 게임 시작 시 호출: 이어하기 or 새 런 ──
    public void StartRun()
    {
        if (!Continue())
        {
            PermanentData meta = SaveManager.LoadPermanent();
            int mainId   = meta != null ? meta.mainWeaponID : defaultMainWeaponId;
            int subId    = meta != null ? meta.subWeaponID  : defaultSubWeaponId;
            int[] skills = meta != null && meta.skillIDs != null ? meta.skillIDs : defaultSkillIds;

            StartNewRun(mainId, subId, skills);
        }

        GameManager.Instance.StartGame();   // 이어하기든 새 런이든 항상
    }

    // ── 새 런
    public void StartNewRun(int mainWeaponId, int subWeaponId, int[] skillIds)
    {
        playerAttack.InitWeapons(database.GetWeapon(mainWeaponId), database.GetWeapon(subWeaponId));
        skillController.InitSkills(GetSkills(skillIds));
        UpgradeManager.Instance.ResetRun();

        // TODO: 플레이어 HP 등 시작값 세팅, 던전 생성

        Save();
    }

    // ── 저장: 각자한테 값 모으기 ──
    public void Save()
    {
        RunSaveData data = new RunSaveData();

        // 장비
        data.loadout.mainWeaponId = playerAttack.MainWeapon != null ? playerAttack.MainWeapon.weapon_ID : 0;
        data.loadout.subWeaponId  = playerAttack.SubWeapon  != null ? playerAttack.SubWeapon.weapon_ID  : 0;
        data.loadout.skillIds = skillController.GetSkillIds();

        // 강화 (복사해서 넣기)
        data.upgrades = new List<int>(UpgradeManager.Instance.upgrades);

        // 쿨타임
        data.skillSlots = skillController.GetSlotSaves();

        // 플레이어 상태
        data.player.hp = playerHealth.Current;

        // 던전
        data.dungeon.roomIndex = StageManager.Instance.roomIndex;

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        Debug.Log($"[RunSave] hp={playerHealth.Current}, room={StageManager.Instance.roomIndex}", playerHealth);
        Debug.Log($"[RunSave] 저장: {SavePath}");
    }

    public bool Continue()
    {
        RunSaveData data = Load();
        if(data == null ) return false;

        // 1. 장착 ID로 원본 찾아 복사본 생성
        playerAttack.InitWeapons(database.GetWeapon(data.loadout.mainWeaponId),
                                 database.GetWeapon(data.loadout.subWeaponId));
        skillController.InitSkills(GetSkills(data.loadout.skillIds));

        // 2. 강화 다시 적용
        UpgradeManager.Instance.Replay(data.upgrades, database.UpgradeLookup);

        // 3. 상태 복원 (반드시 강화 뒤 — 최대치가 바뀌니까)
        skillController.RestoreSlots(data.skillSlots);

        Debug.Log($"[RunSave] 이어하기 (강화 {data.upgrades.Count}개)");

        float hp = Mathf.Clamp(data.player.hp, 1, playerHealth.Max);
        playerHealth.Init(playerHealth.Max, hp);

        StageManager.Instance.roomIndex = data.dungeon.roomIndex;
        Debug.Log($"[RunSave] 이어하기 무기: main={data.loadout.mainWeaponId}, sub={data.loadout.subWeaponId}");

        return true;
    }
    public RunSaveData Load()
    {
        if(!HasSave()) return null;
        return JsonUtility.FromJson<RunSaveData>(File.ReadAllText(SavePath));
    }

    public void Delete()
    {
        if(HasSave()) File.Delete(SavePath);
    }

    SkillData[] GetSkills(int[] ids)
    {
        SkillData[] result = new SkillData[PlayerSkillController.SlotCount];
        for (int i = 0; i < result.Length && ids != null && i < ids.Length; i++)
            result[i] = database.GetSkill(ids[i]);
        return result;
    }
}
