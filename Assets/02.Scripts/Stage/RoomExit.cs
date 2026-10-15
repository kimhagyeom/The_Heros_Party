using UnityEngine;

public class RoomExit : MonoBehaviour
{
    [SerializeField] private Room nextRoom;
    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player")) return;

        StageManager.Instance.RequestMoveTo(nextRoom);
    }
}
