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

        if (hpText != null)
            hpText.text = $"{Mathf.CeilToInt(boss.health.Current):N0} / {Mathf.CeilToInt(boss.health.Max):N0}";
    }
}
