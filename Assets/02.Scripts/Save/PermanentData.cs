// 영구 저장 데이터
[System.Serializable]
public class PermanentData
{
    // 스탯
    public float maxHealth;
    public float moveSpeed;
    public float baseAtk;
    public float maxStamina;

    // 장비 (ID로 저장)
    public int mainWeaponID;
    public int subWeaponID;
    public int[] skillIDs;
}
