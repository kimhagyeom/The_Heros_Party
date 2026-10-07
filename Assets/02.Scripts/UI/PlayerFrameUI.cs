using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerFrameUI : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;

    private Health health;

    void Start()
    {
        health = player.GetComponent<Health>();
        nameText.text = player.stats.Character_Name;
    }

    void Update()
    {
        hpSlider.value = health.Current / health.Max;
        staminaSlider.value = player.stats.currentStamina / player.stats.maxStamina;
    }
}