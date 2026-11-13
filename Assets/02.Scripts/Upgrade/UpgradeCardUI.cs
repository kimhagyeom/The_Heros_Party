using UnityEngine;

public class UpgradeCardUI : MonoBehaviour
{
    [SerializeField] private UpgradeData upgradeData;
    public void SetUpgrade()
    {
        UpgradeManager.Instance.UpgradeSkill(upgradeData);
    }
}
