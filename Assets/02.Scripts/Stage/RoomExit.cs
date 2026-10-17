using UnityEngine;

public class RoomExit : MonoBehaviour
{
    [SerializeField] private Room nextRoom;
    [SerializeField] private GameObject fog1;
    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player")) return;

        StageManager.Instance.RequestMoveTo(nextRoom);
    }
    public void DisableFog()
    {
        fog1.SetActive(false);
    }
}
