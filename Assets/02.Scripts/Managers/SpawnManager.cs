using System;
using System.Collections;
using UnityEngine;

public class SpawnManager : Singleton<SpawnManager>
{
    [Header("스폰 연출")]
    [SerializeField] private GameObject spawnEffect;
    [SerializeField] private float spawnEffectDelay = 0.8f;
    // 리스트 순서대로 스폰 (월드좌표)
    [SerializeField] private SpawnEntry[] spawnEntries;

    private int pendingSpawnCount = 0;

    public void StartSpawning(SpawnEntry[] spawnEntries, Transform origin, Action onTotalDead)
    {
        if(spawnEntries == null || origin == null)
        {
            Debug.LogWarning("SpawnEntry 나 origin가 비어있음",this);
        }
        StartCoroutine(SpawnWaveRoutine(spawnEntries,origin,onTotalDead));
    }
    public void StopAll() //플레이어 사망시
    {
        StopAllCoroutines();
        pendingSpawnCount = 0;
    }

    private IEnumerator SpawnWaveRoutine(SpawnEntry[] spawnEntries, Transform origin, Action onTotalDead)
    {
        foreach (SpawnEntry entry in spawnEntries)
        {
            if (entry.delay > 0f) yield return new WaitForSeconds(entry.delay);
            Vector3 worldPos = origin.TransformPoint(entry.position);

            StartCoroutine(SpawnWithEffect(entry.enemyPrefab, worldPos));
        }
        yield return new WaitUntil(()=> pendingSpawnCount == 0);

        // Enemy는 Start()에서 Register하므로 한 프레임 기다려야 방금 스폰한 적이 카운트에 잡힘
        yield return null;

        // Enemy.Die()가 EnemyRegistry.Unregister()를 호출하는 것에 의존함 -나중에 적 풀링(재사용)으로 바꾸면 Unregister 타이밍도 맞춰줘야 함
        yield return new WaitUntil(() => EnemyRegistry.Instance.ActiveEnemies.Count == 0);
        //GameManager.Instance.ClearStage();
        onTotalDead?.Invoke();
    }

    IEnumerator SpawnWithEffect(Enemy enemyPrefab, Vector3 worldPos)
    {
        if(spawnEffect != null)
            Instantiate(spawnEffect, worldPos, Quaternion.identity);
        

        pendingSpawnCount++;
        if(spawnEffectDelay > 0f) yield return new WaitForSeconds(spawnEffectDelay);
        
        Instantiate(enemyPrefab, worldPos, Quaternion.identity);
        pendingSpawnCount--;
    }
    // 씬 뷰에서 스폰 위치 확인용
    private void OnDrawGizmos()
    {
        if (spawnEntries == null) return;

        foreach (SpawnEntry entry in spawnEntries)
        {
            Gizmos.color = entry.enemyPrefab != null ? Color.red : Color.gray;
            Gizmos.DrawWireSphere(entry.position, 0.5f);
        }
    }
}
