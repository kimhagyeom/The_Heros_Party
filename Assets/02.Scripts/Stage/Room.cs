using System;
using Thry.ThryEditor.Drawers;
using UnityEngine;

[System.Serializable]
public class SpawnEntry
{
    public Enemy enemyPrefab;
    public Vector3 position;
    [Tooltip("이전 스폰 이후 이 적이 스폰되기까지 대기 시간(초)")]
    public float delay;
}
public enum RoomType
{
    Enemies,
    Interact,
    Boss
}

public class Room : MonoBehaviour
{
    [Header("도착 위치")]
    [SerializeField] private Transform arrivalPoint;
    [Header("클리어 조건")]
    [SerializeField] private RoomType roomType;

    [SerializeField] private SpawnEntry[] spawnEntries;
    [SerializeField] private RoomExit roomExit;

    [SerializeField] private BossController boss;


    public bool isCleared {get; private set;}
    public Vector3 ArrivalPosition => arrivalPoint != null ? arrivalPoint.position : transform.position;
    public event Action<Room> OnRoomCleared;
    
    public void StartCombat()
    {
        if (isCleared) return;
        if(roomExit == null) 
        {
            Debug.Log("RoomExit is null");
        }
        // else
        // {
        //     roomExit.gameObject.SetActive(false);
        // }
        switch (roomType)
        {
            case RoomType.Enemies : 
                if(spawnEntries == null || spawnEntries.Length == 0)
                {
                    Debug.LogWarning("SpawnEntry가 비어있음", this);
                    ClearRoom();
                    return;
                }
                SpawnManager.Instance.StartSpawning(spawnEntries, transform, ClearRoom);
                break;
            case RoomType.Interact : // TODO : RewardChest 후 처리
                break;
            //bossroom
            case RoomType.Boss:
                if (boss == null)
                {
                    Debug.LogWarning("Boss가 비어있음", this); return;
                }
                boss.StartBattle();
                break;
        }
    }
    public void ClearRoom()
    {
        if(isCleared) return;
        Debug.Log("Cleared",this);
        isCleared = true;
        OnRoomCleared?.Invoke(this);
        if(roomExit == null) 
        {
            Debug.Log("RoomExit is null" + "게임 클리어!");
        }
        else
        {
            roomExit.DisableFog();
        }
    }
    //플레이어 사망 리셋 시 처음 상태 호출
    public void ResetRoom()
    {
        isCleared = false;
        if (roomExit != null) roomExit.EnableFog();
    }
    private void OnDrawGizmos()
    {
        if (spawnEntries == null) return;
 
        foreach (SpawnEntry entry in spawnEntries)
        {
            if (entry == null) continue;
            Gizmos.color = entry.enemyPrefab != null ? Color.red : Color.gray;
            Gizmos.DrawWireSphere(transform.TransformPoint(entry.position), 5f);
        }
    }
}
