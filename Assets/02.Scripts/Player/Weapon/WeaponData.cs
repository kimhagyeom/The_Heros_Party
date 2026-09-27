using UnityEngine;

public enum Weapon_Type
{
    Sword,
    Raiper,
    Greatsword,
    Trowin_Dagger
}
public enum Weapon_Slot_Type{ Main, Sub }
public enum Atk_Type { Melee, projectile, Aoe }
public enum Hitbox_Shape{ Sector, Box }
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
    public Atk_Type atk_Type;
    public Hitbox_Shape hitbox_Shape;
    public float Angle;
}
