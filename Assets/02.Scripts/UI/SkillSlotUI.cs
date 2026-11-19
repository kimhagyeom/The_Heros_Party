using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI stackText;

    private SkillSlot slot;

    public void Bind(SkillSlot slot)
    {
        this.slot = slot;
    }

    void Update()
    {
        if (slot == null || slot.IsEmpty)
        {
            cooldownOverlay.fillAmount = 0f;
            stackText.text = "";
            return;
        }

        // 덜 찼으면 충전 진행도 표시, 가득이면 0
        cooldownOverlay.fillAmount = slot.IsFull ? 0f : slot.rechargeRemaining / slot.data.cooldown;

        // 충전형만 숫자 표시
        stackText.text = slot.MaxStacks > 1 ? slot.stacks.ToString() : "";
    }
}
