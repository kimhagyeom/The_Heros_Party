using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private BossController boss;
    [SerializeField] private GameObject root;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text hpText; // "2,468 / 2,500"
    [SerializeField] private Slider shieldSlider;

    void Start()
    {
        if (boss != null && nameText != null)
            nameText.text = boss.Data.enemy_Name;
    }

    void Update()
    {
        bool show = boss != null && boss.IsInBattle;
        if (root.activeSelf != show) root.SetActive(show);
        if (!show || boss.health.Max <= 0f) return;

        hpSlider.value = boss.health.Current / boss.health.Max;

        bool shielded = boss.health.IsShielded && boss.health.ShieldMax > 0f;
        if (shieldSlider != null)
        {
            if (shieldSlider.gameObject.activeSelf != shielded)
                shieldSlider.gameObject.SetActive(shielded);
            if (shielded)
                shieldSlider.value = boss.health.shieldHP / boss.health.ShieldMax;
        }
        if (hpText != null)
        {
            hpText.text = $"{Mathf.CeilToInt(boss.health.Current):N0} / {Mathf.CeilToInt(boss.health.Max):N0}";
            hpText.color = shielded ? Color.black : Color.white;
        }
    }
}
