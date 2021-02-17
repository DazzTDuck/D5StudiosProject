using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class EnemySpawnerInstatiator : MonoBehaviour
{
    [SerializeField] GameObject enemySpawner;
    [SerializeField] Transform[] targetPoints;
    [SerializeField] Transform[] spawnPoints;

    [Space, SerializeField] GameObject gateHealth;
    [SerializeField] Transform instatiatePoint;

    private void Start()
    {
        var reference = BoltNetwork.Instantiate(enemySpawner, Vector3.zero, Quaternion.identity);
        var enemySpawning = reference.GetComponent<EnemySpawningHandler>();
        GetComponent<NetworkCallbacks>().enemySpawning = enemySpawning;
        enemySpawning.targetPoints = targetPoints;
        enemySpawning.spawnPoints = spawnPoints;

        if (!FindObjectOfType<GateHealth>())
            BoltNetwork.Instantiate(gateHealth, instatiatePoint.position, Quaternion.identity);
    }
}
