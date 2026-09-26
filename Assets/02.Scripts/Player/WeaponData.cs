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
    public int weapon_ID;
    public string weapon_Name;
    public Weapon_Type weapon_Type;
    public Weapon_Slot_Type weapon_Slot_Type;
    public int combo_Count;
    public float atk_1_rate;
    public float atk_2_rate;
    public float atk_3_rate;
    public float atk_Range;
    public float atk_1_Time;
    public float atk_2_Time;
    public float atk_3_Time;
    public Atk_Type atk_Type;
    public Hitbox_Shape hitbox_Shape;
    public float Angle;
}
