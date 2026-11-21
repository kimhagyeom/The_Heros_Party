using UnityEngine;
using UnityEngine.InputSystem;

// 베이스캠프 무기고 / 범위 안에서 E 누르면 장비 패널 열고 닫기
public class Armory : MonoBehaviour
{
    [SerializeField] private GameObject equipPanel;

    private bool isPlayerNear = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNear = false;
    }

    void Update()
    {
        if (!isPlayerNear) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            bool isOpen = equipPanel.activeSelf;
            equipPanel.SetActive(!isOpen);
        }
    }
}
