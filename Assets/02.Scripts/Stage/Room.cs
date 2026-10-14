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
    Interact
}

public class Room : MonoBehaviour
{
    [Header("도착 위치")]
    [SerializeField] private Transform arrivalPoint;
    [Header("클리어 조건")]
    [SerializeField] private RoomType roomType;

    [SerializeField] private SpawnEntry[] spawnEntries;

    public bool isCleared {get; private set;}
    public Vector3 ArrivalPosition => arrivalPoint != null ? arrivalPoint.position : transform.position;
    public event Action<Room> OnRoomCleared;
    
    public void StartCombat()
    {
        if (isCleared) return;
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
        }
    }
    public void ClearRoom()
    {
        if(isCleared) return;
        isCleared = true;
        OnRoomCleared?.Invoke(this);
    }
}
