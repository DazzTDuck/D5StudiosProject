using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Bolt;
using UdpKit;

public class EnemySpawningHandler : GlobalEventListener
{
    [SerializeField] float spawnPerSecond = 0.75f;
    [SerializeField] int maxEnemies = 5;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] Transform[] targetPoints;
    public List<GameObject> enemies = new List<GameObject>();

    bool startSpawning = false;
    float spawnTimer;

    private void Start()
    {
        StartSpawning();
        Debug.LogWarning("started");
    }

    private void Update()
    {
        UpdateSpawnTimer();
    }

    public void UpdateSpawnTimer()
    {
        if(enemies.Count < maxEnemies && startSpawning)
        {
            spawnTimer -= BoltNetwork.FrameDeltaTime;

            if (spawnTimer <= 0)
            {
                SpawnEnemy();
                spawnTimer = spawnPerSecond;
            }
        }
    }

    public void SpawnEnemy()
    {
        var enemy = BoltNetwork.Instantiate(enemyPrefab, GetRandomSpawnPoint(), Quaternion.identity);
        //var enemy = Instantiate(enemyPrefab, GetRandomSpawnPoint(), Quaternion.identity);
        enemies.Add(enemy.gameObject);
        var agent = enemy.gameObject.AddComponent<NavMeshAgent>();
        enemy.GetComponent<EnemyMove>().SetAgent(agent);
        enemy.GetComponent<EnemyMove>().SetPath(GetRandomTargetPoint());
    }

    public Vector3 GetRandomSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)].position;
    }
    public Vector3 GetRandomTargetPoint()
    {
        return targetPoints[Random.Range(0, targetPoints.Length)].position;
    }

    public void RemoveEnemyFromList(GameObject enemy)
    {
        var index = enemies.IndexOf(enemy);
        enemies.RemoveAt(index);
    }

    public void StartSpawning() { startSpawning = true; spawnTimer = spawnPerSecond; }
}
