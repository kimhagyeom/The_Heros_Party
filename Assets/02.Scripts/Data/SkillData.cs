using UnityEngine;

public enum SkillSlot { Q, E, Shift, R }
public enum SkillActionType { Dash } // 지금은 이거 하나만

[System.Serializable]
public struct SkillEffectParams
{
    public float distance;
    public float duration;
}

[CreateAssetMenu(menuName = "SkillData")]
public class SkillData : ScriptableObject
{
    public int skill_ID;
    public string skill_Name;
    public SkillSlot slot;
    public float cooldown = 3f;
    public float stamina_Cost = 20f;

    public SkillActionType actionType;
    public SkillEffectParams effectParams;
}