using UnityEngine;
using System.Collections.Generic;
public class UpgradeSelector : Singleton<UpgradeSelector>
{
    [SerializeField] private UpgradeData[] upgrades;
    [SerializeField] private UpgradeCardUI[] upgradeCardUIs;
    public GameObject upgradePanel;

    private List<UpgradeData> selectedUpgrades = new List<UpgradeData>();

    public List<UpgradeData> GetSelectedUpgrades(int count)
    {
        // 일반 강화 후보만
        List<UpgradeData> randomUpgrades = new List<UpgradeData>();
        foreach (var u in upgrades)
        {
            if (!u.is_Enabled) continue;
            if (u.upgrade_Grade != UpgradeGrade.Normal) continue;
            if (!UpgradeManager.Instance.IsEquipped(u)) continue; //장착 안한 무기 스킬 빼기  
            if (UpgradeManager.Instance.GetProgress(u.target_ID).IsComplete) continue; //강화 완료된거 빼기
            randomUpgrades.Add(u);
        }

        for(int i = randomUpgrades.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1); // 0~i
            UpgradeData temp = randomUpgrades[i];
            randomUpgrades[i] = randomUpgrades[r];
            randomUpgrades[r] = temp;
        }

        int pickCount = Mathf.Min(count, randomUpgrades.Count);
        return randomUpgrades.GetRange(0, pickCount);
    }

    public void ShowUpgradeOptions(int count)
    {
        selectedUpgrades = GetSelectedUpgrades(count);
        ShowCards(selectedUpgrades);
    }

    // 카드 클릭
    public void Pick(UpgradeData upgradeData)
    {
        UpgradeManager.Instance.UpgradeSkill(upgradeData);

        // 3회마다 특수 강화
        if (upgradeData.upgrade_Grade == UpgradeGrade.Normal && UpgradeManager.Instance.NeedSpecial(upgradeData.target_ID))
        {
            ShowCards(GetSpecialCandidates(upgradeData.target_ID));
            return;
        }
        upgradePanel.SetActive(false);
    }

    void ShowCards(List<UpgradeData> cards)
    {
        if (cards.Count == 0) // 카드 없으면 닫기
        {
            upgradePanel.SetActive(false);
            return;
        }

        for (int i = 0; i < upgradeCardUIs.Length; i++)
        {
            if (i < cards.Count)
            {
                upgradeCardUIs[i].gameObject.SetActive(true);
                upgradeCardUIs[i].SetData(cards[i]);
            }
            else
            {
                upgradeCardUIs[i].gameObject.SetActive(false); // 뽑힌 게 적으면 남는 카드 숨김
            }
        }

        upgradePanel.SetActive(true);
    }

    public List<UpgradeData> GetSpecialCandidates(int targetId)
    {
        UpgradeProgress p = UpgradeManager.Instance.GetProgress(targetId);
        List<UpgradeData> result = new List<UpgradeData>();

        foreach (var u in upgrades)
        {
            if (!u.is_Enabled) continue;
            if (u.upgrade_Grade != UpgradeGrade.Special) continue;
            if (u.target_ID != targetId) continue;                // 이 대상 것만
            if (p.takenSpecials.Contains(u.upgrade_ID)) continue; // 먹은 건 제외
            result.Add(u);
        }
        return result;
    }
}
