using UnityEngine;

public class SkillExecutor : MonoBehaviour
{
    //  스킬 실행 흐름(선딜 → 판정 → 후딜)
    public void Execute(PlayerController player, SkillData data)
    {
        Debug.Log($"{data.skill_Name} 실행 (아직 구현 안 됨)");
    }
}
