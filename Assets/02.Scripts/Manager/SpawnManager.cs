using System.Collections;
using UnityEngine;

public class SpawnManager : Singleton<SpawnManager>
{
    [SerializeField] private Enemy[] enemyPrefabs;
    [SerializeField] private Transform spawnAreaCenter;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private int enemyCountPerWave = 5;
    [SerializeField] private float spawnInterval = 1f;

    public void StartSpawning()
    {
        StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        for (int i = 0; i < enemyCountPerWave; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }

        // Enemy.Die()가 EnemyRegistry.Unregister()를 호출하는 것에 의존함 — 나중에 적 풀링(재사용)으로 바꾸면 Unregister 타이밍도 맞춰줘야 함
        yield return new WaitUntil(() => EnemyRegistry.Instance.ActiveEnemies.Count == 0);
        GameManager.Instance.ClearStage();
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0) return;

        Enemy prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Vector3 center = spawnAreaCenter != null ? spawnAreaCenter.position : transform.position;
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        // insideUnitCircle의 y값을 z에 매핑 (바닥이 XZ 평면이라 y는 항상 0으로 고정)
        Vector3 spawnPos = center + new Vector3(randomOffset.x, 0f, randomOffset.y);

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = spawnAreaCenter != null ? spawnAreaCenter.position : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, spawnRadius);
    }
}
