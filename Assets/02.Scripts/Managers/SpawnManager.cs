using System.Collections;
using UnityEngine;

public class SpawnManager : Singleton<SpawnManager>
{
    [System.Serializable]
    public class SpawnEntry
    {
        public Enemy enemyPrefab;
        public Vector3 position;
        [Tooltip("이전 스폰 이후 이 적이 스폰되기까지 대기 시간(초)")]
        public float delay;
    }

    // 리스트 순서대로 스폰 (월드좌표)
    [SerializeField] private SpawnEntry[] spawnEntries;

    public void StartSpawning()
    {
        StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        foreach (SpawnEntry entry in spawnEntries)
        {
            if (entry.delay > 0f) yield return new WaitForSeconds(entry.delay);
            SpawnEnemy(entry);
        }

        // Enemy는 Start()에서 Register하므로 한 프레임 기다려야 방금 스폰한 적이 카운트에 잡힘
        yield return null;

        // Enemy.Die()가 EnemyRegistry.Unregister()를 호출하는 것에 의존함 -나중에 적 풀링(재사용)으로 바꾸면 Unregister 타이밍도 맞춰줘야 함
        yield return new WaitUntil(() => EnemyRegistry.Instance.ActiveEnemies.Count == 0);
        GameManager.Instance.ClearStage();
    }

    private void SpawnEnemy(SpawnEntry entry)
    {
        if (entry.enemyPrefab == null)
        {
            Debug.LogWarning("[SpawnManager] enemyPrefab이 비어 있는 SpawnEntry가 있음", this);
            return;
        }
        Instantiate(entry.enemyPrefab, entry.position, Quaternion.identity);
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
