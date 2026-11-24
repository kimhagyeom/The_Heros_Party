using UnityEngine;
using System.Collections.Generic;

public class UpgradeSelector : Singleton<UpgradeSelector>
{
    [SerializeField] private GameDatabase databases;          // 강화 목록은 여기서만 읽음
    [SerializeField] private UpgradeCardUI[] upgradeCardUIs;
    [SerializeField] private PlayerController player;
    [SerializeField] private int choiceCount = 3;
    public GameObject upgradePanel;

    // ── 창 열기 ──
    public void ShowUpgradeOptions()
    {
        ShowCards(PickRandom(GetNormalCandidates(), choiceCount));
    }

    void ShowSpecialOptions(int targetId)
    {
        ShowCards(PickRandom(GetSpecialCandidates(targetId), choiceCount));
    }

    // ── 카드 클릭 ──
    public void Pick(UpgradeData upgradeData)
    {
        UpgradeManager.Instance.UpgradeSkill(upgradeData);

        // 일반 강화로 3회 달성하면 특수 강화
        if (upgradeData.upgrade_Grade == UpgradeGrade.Normal &&
            UpgradeManager.Instance.NeedSpecial(upgradeData.target_ID))
        {
            ShowSpecialOptions(upgradeData.target_ID);
            return;
        }

        ClosePanel();
    }

    // ── 후보 거르기 ──
    List<UpgradeData> GetNormalCandidates()
    {
        List<UpgradeData> result = new List<UpgradeData>();

        foreach (var u in databases.upgrades)
        {
            if (u == null || !u.is_Enabled) continue;
            if (u.upgrade_Grade != UpgradeGrade.Normal) continue;
            if (!UpgradeManager.Instance.IsEquipped(u)) continue;                      // 장착 안 한 무기/스킬 제외
            if (UpgradeManager.Instance.GetProgress(u.target_ID).IsComplete) continue; // 강화 완료 제외
            result.Add(u);
        }
        return result;
    }

    public List<UpgradeData> GetSpecialCandidates(int targetId)
    {
        UpgradeProgress p = UpgradeManager.Instance.GetProgress(targetId);
        List<UpgradeData> result = new List<UpgradeData>();

        foreach (var u in databases.upgrades)
        {
            if (u == null || !u.is_Enabled) continue;
            if (u.upgrade_Grade != UpgradeGrade.Special) continue;
            if (u.target_ID != targetId) continue;                // 이 대상 것만
            if (p.takenSpecials.Contains(u.upgrade_ID)) continue; // 먹은 건 제외
            result.Add(u);
        }
        return result;
    }

    // ── 섞어서 count개 (일반/특수 공용) ──
    List<UpgradeData> PickRandom(List<UpgradeData> candidates, int count)
    {
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1); // 0~i
            UpgradeData temp = candidates[i];
            candidates[i] = candidates[r];
            candidates[r] = temp;
        }

        int pickCount = Mathf.Min(count, candidates.Count);
        return candidates.GetRange(0, pickCount);
    }

    // ── 카드 표시 ──
    void ShowCards(List<UpgradeData> cards)
    {
        if (cards.Count == 0) // 뽑을 게 없으면 닫기
        {
            ClosePanel();
            return;
        }

        for (int i = 0; i < upgradeCardUIs.Length; i++)
        {
            bool has = i < cards.Count;
            upgradeCardUIs[i].gameObject.SetActive(has);   // 뽑힌 게 적으면 남는 카드 숨김
            if (has) upgradeCardUIs[i].SetData(cards[i]);
        }

        upgradePanel.SetActive(true);
        SetPlayerInput(false); // 카드 클릭이 공격으로 안 나가게
    }

    void ClosePanel()
    {
        upgradePanel.SetActive(false);
        SetPlayerInput(true);
        
    }

    void SetPlayerInput(bool enabled)
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerController>();

        if (enabled) player.InputActions.Player.Enable();
        else         player.InputActions.Player.Disable();
    }
}