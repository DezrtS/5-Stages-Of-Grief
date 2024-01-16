using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWave : MonoBehaviour
{
    [SerializeField] List<EnemyWaveData> enemyWaves = new();
    [SerializeField] private float spawnRadius = 5;
    [SerializeField] private float spawnOffset = 5;

    private readonly List<Enemy> spawnedEnemies = new();

    private int wave = 0;
    private float currentSpawnOffset = 0;
    private bool waveDefeated = false;
    private bool waveStarted = false;

    private void OnEnable()
    {
        foreach (Enemy enemy in spawnedEnemies)
        {
            enemy.OnEnemyDeath += OnEnemyDeath;
        }
    }

    public void OnEnemyDeath(Enemy enemy)
    {
        if (spawnedEnemies.Contains(enemy))
        {
            enemy.OnEnemyDeath -= OnEnemyDeath;
            spawnedEnemies.Remove(enemy);

            if (spawnedEnemies.Count == 0 && waveStarted)
            {
                currentSpawnOffset = 0;
                wave++;
                SpawnWave(wave);
            }
        }
    }

    private void OnDisable()
    {
        foreach (Enemy enemy in spawnedEnemies)
        {
            enemy.OnEnemyDeath -= OnEnemyDeath;
        }
    }

    public void SpawnWave(int wave)
    {
        if (wave >= enemyWaves.Count)
        {
            waveDefeated = true;
            return;
        }

        List<EnemyData> enemyDatas = enemyWaves[wave].EnemyDatas;

        foreach (EnemyData enemyData in enemyDatas)
        {
            for (int i = 0; i < enemyData.EnemyCount; i++)
            {
                SpawnEnemy(enemyData.EnemyPrefab);
            }
        }
    }

    public void SpawnEnemy(GameObject enemyPrefab)
    {
        GameObject enemyObject = Instantiate(enemyPrefab, transform.position + (Quaternion.Euler(0, currentSpawnOffset, 0) * new Vector3(0, 0, spawnRadius)), Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        spawnedEnemies.Add(enemy);
        enemy.OnEnemyDeath += OnEnemyDeath;
        currentSpawnOffset += spawnOffset;
    }

    public void RestartWave()
    {
        waveDefeated = false;
        waveStarted = false;
        currentSpawnOffset = 0;
        wave = 0;

        KillAllSpawnedEnemies();
    }

    public void KillAllSpawnedEnemies()
    {
        foreach (Enemy enemy in spawnedEnemies)
        {
            enemy.Die();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !waveStarted && !waveDefeated)
        {
            waveStarted = true;
            SpawnWave(0);
        }
    }
}

[Serializable]
public class EnemyWaveData
{
    [SerializeField] private List<EnemyData> enemyDatas = new List<EnemyData>();

    public List<EnemyData> EnemyDatas { get { return enemyDatas; } }
}

[Serializable]
public class EnemyData
{
    [SerializeField] private GameObject enemyPrefab;
    [Range(1, 10)]
    [SerializeField] private int enemyCount = 1;

    public GameObject EnemyPrefab { get { return enemyPrefab; } }
    public int EnemyCount { get { return enemyCount; } }
}