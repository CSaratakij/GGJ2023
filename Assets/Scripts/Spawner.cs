using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private const float FIRST_TIME_SPAWNER_DELAY = 1.0f;

    [Header("Settings")]
    [SerializeField] private Transform spawnOrigin;
    [SerializeField] private float[] spawnInterval;
    [SerializeField] private float[] additionalDelay;
    [SerializeField] private GameObject[] prefabs;

    private float spawnTimer = FIRST_TIME_SPAWNER_DELAY;

    private void Start()
    {
        Random.InitState(Random.Range(int.MinValue, int.MaxValue));
        spawnTimer = (Time.time + FIRST_TIME_SPAWNER_DELAY);
    }

    private void Update()
    {
        if (Time.frameCount % 3 != 0)
        {
            return;
        }

        if (GameMode.IsGameOver)
        {
            return;
        }

        if (Time.time > spawnTimer)
        {
            Spawn();
            RestartDelay();
        }
    }

    private void Spawn()
    {
        int pickPrefabIndex = Random.Range(0, prefabs.Length);
        var prefab = prefabs[pickPrefabIndex];
        Instantiate(prefab, spawnOrigin.transform.position, Quaternion.identity);
    }

    private void RestartDelay()
    {
        int pickDelayA = Random.Range(0, spawnInterval.Length);
        int pickDelayB = Random.Range(0, additionalDelay.Length);

        float delayA = spawnInterval[pickDelayA];
        float delayB = additionalDelay[pickDelayB];

        float delay = (delayA + delayB);
        spawnTimer = (Time.time + delay);
    }
}

