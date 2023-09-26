using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawning : Singleton<EnemySpawning>
{
    [SerializeField] private GameObject enemyPrefab;

    [SerializeField] private float spawnTimeOffset = 0.25f;
    [SerializeField] private float spawnCycleTimeOffset = 1f;
    [SerializeField] private float waveTimeOffset = 5f;

    [SerializeField] private float waveScaler = 1.5f;

    [SerializeField] private List<Vector3> spawnPositions;

    private List<Enemy> spawnedEnemies = new List<Enemy>();

    private int waveCount = 1;
    private float scaledWaveTimeOffset = 0;
    private float catchUpTime = 0;

    private float timeSinceLastSpawn = 0;

    private int enemiesToSpawn = 0;
    private bool isSpawning;

    private void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    public void SpawnEnemy(int amount)
    {
        if (spawnPositions.Count <= 0)
        {
            return;
        }

        enemiesToSpawn += amount;

        if (!isSpawning)
        {
            StartCoroutine(SpawnEnemy());
        }
    }

    public void KillAllEnemies()
    {
        StopAllCoroutines();

        foreach (Enemy enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                enemy.Die();
            }
        }

        spawnedEnemies.Clear();
    }

    public void ResetSpawner()
    {
        waveCount = 1;
        scaledWaveTimeOffset = 0;
        enemiesToSpawn = 0;
        isSpawning = false;

        StartCoroutine(SpawnWaves());
    }

    public void PauseSpawning()
    {
        StopAllCoroutines();
        catchUpTime = Time.timeSinceLevelLoad - timeSinceLastSpawn;
    }

    public void UnPauseSpawning()
    {
        if (isSpawning)
        {
            StartCoroutine(SpawnEnemy());
        }

        StartCoroutine(SpawnWaves());
    }

    public IEnumerator SpawnEnemy()
    {
        isSpawning = true;
        int spawnPositionIndex = 0;

        while (enemiesToSpawn > 0)
        {
            yield return new WaitForSeconds(spawnTimeOffset);

            GameObject enemy = Instantiate(enemyPrefab, spawnPositions[spawnPositionIndex], Quaternion.identity);
            spawnedEnemies.Add(enemy.GetComponent<Enemy>());
            enemiesToSpawn--;
            spawnPositionIndex++;


            if (spawnPositionIndex >= spawnPositions.Count)
            {
                spawnPositionIndex = 0;
                yield return new WaitForSeconds(spawnCycleTimeOffset);
            }
        }

        isSpawning = false;
    }

    public IEnumerator SpawnWaves()
    {
        while (true)
        {
            float waitTime = waveTimeOffset + scaledWaveTimeOffset;
            timeSinceLastSpawn = Time.timeSinceLevelLoad;
            yield return new WaitForSeconds(waitTime - catchUpTime);

            SpawnEnemy(waveCount);
            scaledWaveTimeOffset += waveScaler;
            waveCount++;
            catchUpTime = 0;
        }
    }
}