using UnityEngine;

public enum Weapon_Type
{
    Sword,
    Raiper,
    Greatsword,
    Trowin_Dagger
}
public enum Weapon_Slot_Type{ Main, Sub }
[CreateAssetMenu(menuName = "WeaponData")]
public class WeaponData : ScriptableObject
{
    [System.Serializable]
    public struct ComboStep
    {
        public float damageRate;
        public float duration;
    }
    public int weapon_ID;
    public string weapon_Name;
    public Weapon_Type weapon_Type;
    public Weapon_Slot_Type weapon_Slot_Type;
    public int combo_Count;
    public ComboStep[] combo_Step;
    public float atk_Range;
    public AttackType atk_Type;
    public HitboxShape hitbox_Shape;
    public float Angle;
}
