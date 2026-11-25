using System.Collections.Generic;

[System.Serializable]
public class RunSaveData
{
    public int saveVersion = 1;

    public LoadoutSave loadout = new LoadoutSave();
    public PlayerSave player = new PlayerSave();
    public List<int> upgrades = new List<int>();          // 먹은 강화 ID, 먹은 순서대로 (일반+특수)
    public List<SkillSlotSave> skillSlots = new List<SkillSlotSave>();
    public DungeonSave dungeon = new DungeonSave();
}

// 런에 들고 들어온 장비
[System.Serializable]
public class LoadoutSave
{
    public int mainWeaponId;
    public int subWeaponId;
    public int[] skillIds = new int[4];   // 빈 슬롯은 0
}

// 강화로 다시 계산되지 않는 "현재 상태" 값만
[System.Serializable]
public class PlayerSave
{
    public float hp;
    public float stamina;
    public float infection;
}

// 슬롯 순서대로 저장 (skillIds와 같은 인덱스)
[System.Serializable]
public class SkillSlotSave
{
    public int stacks;
    public float rechargeRemaining;
}

[System.Serializable]
public class DungeonSave
{
    public int seed;
    public int floor;
    public int roomIndex;
    public List<int> clearedRooms = new List<int>();
}