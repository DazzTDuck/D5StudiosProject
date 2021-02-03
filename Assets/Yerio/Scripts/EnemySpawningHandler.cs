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
    static bool startGame = false;
    bool gameStarted = false;
    bool isHost = false;
    float spawnTimer;
    PlayerController player;

    public static void StartGame()
    {
        startGame = true;
    }

    public bool GetIfGameStarted()
    {
        return gameStarted;
    }

    public override void SimulateOwner()
    {
        if (isHost)
            UpdateSpawnTimer();

        if (startGame && !gameStarted)
        {
            StartCoroutine(StartDelay());
            startGame = false;
            gameStarted = true;
        }
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

    public void SetPlayer(PlayerController playerController)
    {
        player = playerController;
    }

    IEnumerator StartDelay()
    {
        yield return new WaitUntil(() => player);

        isHost = player.GetIfHost();

        yield return new WaitForSeconds(1);

        if (isHost && entity.IsOwner)
        {
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
