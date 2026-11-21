using TMPro;
using UnityEngine;

public class BaseCampUI : MonoBehaviour
{
    [Header("플레이어 (Player 오브젝트를 끌어다 넣기)")]
    [SerializeField] private Player player;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private PlayerSkillController skillController;
    [SerializeField] private PlayerDataLoader dataLoader;

    [Header("현재 상태를 보여줄 텍스트")]
    [SerializeField] private TMP_Text weaponText;
    [SerializeField] private TMP_Text skillText;
    [SerializeField] private TMP_Text statText;

    private int selectedSlot = 0; // 스킬 넣을 슬롯 (0~3)

    //패널 켜짐/꺼짐
    void OnEnable()
    {
        if (playerController.InputActions != null)
            playerController.InputActions.Player.Disable(); // 클릭이 공격으로 안 나가게
        Refresh();
    }

    void OnDisable()
    {
        if (playerController.InputActions != null)
            playerController.InputActions.Player.Enable();

        dataLoader.Save(); // 패널 닫을 때 영구 저장
    }

    //무기 버튼용
    public void EquipMainWeapon(WeaponData weapon)
    {
        playerAttack.SetMainWeapon(weapon);
        Refresh();
    }

    public void EquipSubWeapon(WeaponData weapon)
    {
        playerAttack.SetSubWeapon(weapon);
        Refresh();
    }

    //스킬 버튼용
    public void SelectSlot(int slotIndex)// 슬롯 버튼: 0~3
    {
        selectedSlot = slotIndex;
        Refresh();
    }

    public void EquipSkill(SkillData skill)// 스킬 버튼
    {
        skillController.SetSkill(selectedSlot, skill);
        Refresh();
    }

    public void ClearSkill()
    {
        skillController.SetSkill(selectedSlot, null);
        Refresh();
    }
    //스탯 버튼용 + -
    public void AddAtk(float amount) { playerController.stats.base_Atk = Mathf.Max(1f, playerController.stats.base_Atk + amount); ApplyStats(); }
    public void AddMoveSpeed(float amount) { playerController.stats.Move_Speed = Mathf.Max(1f, playerController.stats.Move_Speed + amount); ApplyStats(); }
    public void AddMaxHealth(float amount) { playerController.stats.maxHealth = Mathf.Max(1f, playerController.stats.maxHealth + amount); ApplyStats(); }
    public void AddMaxStamina(float amount) { playerController.stats.maxStamina = Mathf.Max(1f, playerController.stats.maxStamina + amount); ApplyStats(); }

    void ApplyStats()
    {
        player.RefreshStats(); // 체력/스태미나 다시 채움
        Refresh();
    }

    void Refresh()
    {
        if (weaponText != null)
            weaponText.text = $"주무기 : {WeaponName(playerAttack.mainWeaponData)}\n" +
                              $"보조무기 : {WeaponName(playerAttack.subWeaponData)}";

        if (skillText != null)
        {
            string text = "";
            for (int i = 0; i < PlayerSkillController.SlotCount; i++)
            {
                string mark = i == selectedSlot ? "▶ " : "   ";
                text += $"{mark}{i + 1}번 : {SkillName(skillController.GetSlot(i))}\n";
            }
            skillText.text = text;
        }

        if (statText != null)
        {
            PlayerStats s = playerController.stats;
            statText.text = $"공격력 : {s.base_Atk:0.#}\n" +
                            $"이동속도 : {s.Move_Speed:0.#}\n" +
                            $"최대 체력 : {s.maxHealth:0.#}\n" +
                            $"최대 스태미나 : {s.maxStamina:0.#}";
        }
    }

    string WeaponName(WeaponData w) => w != null ? w.weapon_Name : "(없음)";
    string SkillName(SkillSlot slot) => slot != null && !slot.IsEmpty ? slot.data.skill_Name : "비어있음";
}