using UnityEngine;

// 프로토타입용 스킬 상태 표시: Canvas 없이 OnGUI로 화면 왼쪽 아래에 그림
// Player 오브젝트에 붙이면 됨. 정식 UI 만들면 교체
public class SkillHUD : MonoBehaviour
{
    [SerializeField] private PlayerSkillController skillController;
    [SerializeField] private int fontSize = 20;
    [SerializeField] private float width = 420f;

    private GUIStyle style;

    void Awake()
    {
        if (skillController == null)
            skillController = GetComponent<PlayerSkillController>();
    }

    void OnGUI()
    {
        if (skillController == null) return;

        if (style == null)
            style = new GUIStyle(GUI.skin.label) { fontSize = fontSize, richText = true };

        float lineHeight = fontSize + 8f;
        float height = lineHeight * PlayerSkillController.SlotCount + 16f;
        Rect box = new Rect(10f, Screen.height - height - 10f, width, height);
        GUI.Box(box, GUIContent.none);

        for (int i = 0; i < PlayerSkillController.SlotCount; i++)
        {
            Rect line = new Rect(box.x + 10f, box.y + 8f + i * lineHeight, width - 20f, lineHeight);
            GUI.Label(line, GetSlotText(i), style);
        }
    }

    // 예: [2] 일섬  1/2  준비  (5.3s)
    string GetSlotText(int index)
    {
        string key = $"[{index + 1}]";
        SkillSlot slot = skillController.GetSlot(index);
        if (slot == null || slot.IsEmpty)
            return $"{key} <color=grey>(비어있음)</color>";

        string text = $"{key} {slot.data.skill_Name}  ";

        if (slot.MaxStacks > 1)
            text += $"{slot.stacks}/{slot.MaxStacks}  ";

        if (skillController.CastingSlot == index)
            text += $"<color=yellow>▶ {PlayerSkillController.GetPhaseName(skillController.Phase)}</color>";
        else if (slot.stacks > 0)
            text += "<color=lime>준비</color>";
        else
            text += "<color=red>쿨타임</color>";

        if (!slot.IsFull)
            text += $"  ({slot.rechargeRemaining:0.0}s)";

        return text;
    }
}
