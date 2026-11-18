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
        List<UpgradeData> randomUpgrades = new List<UpgradeData>(upgrades);
        
        for(int i = upgrades.Length - 1; i > 0; i--)
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

        for (int i = 0; i < upgradeCardUIs.Length; i++)
        {
            if (i < selectedUpgrades.Count)
            {
                upgradeCardUIs[i].gameObject.SetActive(true);
                upgradeCardUIs[i].SetData(selectedUpgrades[i]);
            }
            else
            {
                upgradeCardUIs[i].gameObject.SetActive(false); // 뽑힌 게 적으면 남는 카드 숨김
            }
        }

        upgradePanel.SetActive(true);
    }
}
