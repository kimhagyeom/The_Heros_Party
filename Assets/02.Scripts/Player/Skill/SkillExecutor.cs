using UnityEngine;
using System.Collections;

public class SkillExecutor : MonoBehaviour
{
    public void Execute(PlayerController player, SkillData data)
    {
        switch (data.actionType)
        {
            case SkillActionType.Dash:
                player.StartCoroutine(DashEffect(player, data));
                break;
        }
    }

    IEnumerator DashEffect(PlayerController player, SkillData data)
    {
        Debug.Log($"{data.skill_Name} 돌진! 거리 {data.effectParams.distance}, 시간 {data.effectParams.duration}");
        // 실제 이동 로직은 필요할 때 채우기
        yield return null;
    }
}