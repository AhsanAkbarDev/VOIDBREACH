using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject scourgePrefab;
    public GameObject harbingerPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Player")]
    public Movement player;

    [Header("Wave Settings")]
    public float timeBetweenSpawns = 2f;
    public float timeBetweenWaves = 4f;

    [Header("Wave Tracking")]
    public int waveNumber;
    public bool victory;

    private int enemiesAlive;

    private bool spawnerHasStarted;
    private bool waveInProgress;
    private bool finishedSpawning;

    private readonly List<GameObject> spawnedEnemies =
        new List<GameObject>();

    private void Update()
    {
        if (spawnerHasStarted ||
            player == null)
        {
            return;
        }

        if (player.powerSurgeActivated)
        {
            spawnerHasStarted = true;
            StartCoroutine(StartNextWave());
        }
    }

    private IEnumerator StartNextWave()
    {
        waveNumber++;

        waveInProgress = true;
        finishedSpawning = false;

        switch (waveNumber)
        {
            case 1:
                yield return StartCoroutine(
                    SpawnEnemies(
                        scourgePrefab,
                        3
                    )
                );
                break;

            case 2:
                yield return StartCoroutine(
                    SpawnEnemies(
                        scourgePrefab,
                        4
                    )
                );

                yield return StartCoroutine(
                    SpawnEnemies(
                        harbingerPrefab,
                        1
                    )
                );
                break;

            case 3:
                yield return StartCoroutine(
                    SpawnEnemies(
                        scourgePrefab,
                        3
                    )
                );

                yield return StartCoroutine(
                    SpawnEnemies(
                        harbingerPrefab,
                        2
                    )
                );
                break;

            default:
                CompleteAllWaves();
                yield break;
        }

        finishedSpawning = true;
        CheckWaveComplete();
    }

    private IEnumerator SpawnEnemies(
        GameObject enemyPrefab,
        int amount)
    {
        if (enemyPrefab == null ||
            spawnPoints.Length == 0)
        {
            yield break;
        }

        for (int i = 0; i < amount; i++)
        {
            Transform spawnPoint =
                spawnPoints[
                    Random.Range(
                        0,
                        spawnPoints.Length
                    )
                ];

            GameObject enemyObject =
                Instantiate(
                    enemyPrefab,
                    spawnPoint.position,
                    spawnPoint.rotation
                );

            spawnedEnemies.Add(enemyObject);
            enemiesAlive++;

            AssignSpawnerReference(enemyObject);

            yield return new WaitForSeconds(
                timeBetweenSpawns
            );
        }
    }

    private void AssignSpawnerReference(
        GameObject enemyObject)
    {
        Enemy scourge =
            enemyObject.GetComponent<Enemy>();

        if (scourge != null)
        {
            scourge.enemySpawner = this;
        }

        Harbinger harbinger =
            enemyObject.GetComponent<Harbinger>();

        if (harbinger != null)
        {
            harbinger.enemySpawner = this;
        }
    }

    public void EnemyDied()
    {
        enemiesAlive--;

        enemiesAlive =
            Mathf.Max(
                enemiesAlive,
                0
            );

        CheckWaveComplete();
    }

    private void CheckWaveComplete()
    {
        if (!waveInProgress ||
            !finishedSpawning ||
            enemiesAlive > 0)
        {
            return;
        }

        waveInProgress = false;

        StartCoroutine(WaveDelay());
    }

    private IEnumerator WaveDelay()
    {
        yield return new WaitForSeconds(
            timeBetweenWaves
        );

        StartCoroutine(StartNextWave());
    }

    private void CompleteAllWaves()
    {
        waveInProgress = false;
        victory = true;
    }

    public void ResetSpawner()
    {
        StopAllCoroutines();

        DestroySpawnedEnemies();

        waveNumber = 0;
        enemiesAlive = 0;

        spawnerHasStarted = false;
        waveInProgress = false;
        finishedSpawning = false;

        victory = false;

        if (player != null)
        {
            player.powerSurgeActivated = false;
        }
    }

    private void DestroySpawnedEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }

        spawnedEnemies.Clear();
    }
}
