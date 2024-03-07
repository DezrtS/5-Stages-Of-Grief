using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffectManager : Singleton<ParticleEffectManager>
{
    [Header("Environmental Effects")]
    [SerializeField] private GameObject windNoLoop;
    [SerializeField] private GameObject windWithLoop;
    [SerializeField] private float windYSpawnPos = 10;
    [SerializeField] private float windSpawnRange = 10;
    [SerializeField] private float windSpawnRate = 5;
    [SerializeField] private float windSpawnRateVariation = 2;
    [Range(0,1)]
    [SerializeField] private float normalVsLoopWindChance = 0.75f;

    private Transform playerTransform;

    private float windTimer = 0;

    private void Start()
    {
        playerTransform = PlayerController.Instance.transform;
        windTimer = windSpawnRate;
    }

    private void FixedUpdate()
    {
        windTimer -= Time.fixedDeltaTime;
        if (windTimer <= 0)
        {
            SpawnWind();
            windTimer = windSpawnRate + Random.Range(-windSpawnRateVariation, windSpawnRateVariation);
        }
    }

    private void SpawnWind()
    {
        Vector3 randomPos = playerTransform.position + new Vector3(Random.Range(-windSpawnRange, windSpawnRange) - windYSpawnPos, windYSpawnPos, Random.Range(-windSpawnRange, windSpawnRange) - windYSpawnPos);

        if (Random.Range(0f, 1f) <= normalVsLoopWindChance)
        {
            Instantiate(windNoLoop, randomPos, Quaternion.identity);
        }
        else
        {
            Instantiate(windWithLoop, randomPos, Quaternion.identity);
        }
    }
}