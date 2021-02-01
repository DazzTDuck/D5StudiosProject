using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Bolt;
using UdpKit;

[BoltGlobalBehaviour(BoltNetworkModes.Server)]
public class EnemySpawningHandler : Bolt.EntityBehaviour<IEnemySpawner>
{
    [SerializeField] float spawnPerSecond = 0.75f;
    [SerializeField] int maxEnemies = 5;
    [SerializeField] GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public Transform[] targetPoints;
    public List<GameObject> enemies = new List<GameObject>();

    bool startSpawning = false;
    bool hasInstantiated = false;
    float spawnTimer;

    public override void Attached()
    {
        StartCoroutine(StartDelay());
    }

    void SpawnedSpawnerCallback()
    {
        hasInstantiated = state.HasSpawned;
    }

    public override void SimulateOwner()
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
        //var agent = enemy.gameObject.AddComponent<NavMeshAgent>();
        //enemy.GetComponent<EnemyMove>().SetAgent(agent);
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

    IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(5);

        if (GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerController>().GetIfHost() && entity.IsOwner)
        {
            state.HasSpawned = hasInstantiated;
            state.AddCallback("HasSpawned", SpawnedSpawnerCallback);
            state.HasSpawned = true;
            StartSpawning();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void StartSpawning() { startSpawning = true; spawnTimer = spawnPerSecond; }
}
