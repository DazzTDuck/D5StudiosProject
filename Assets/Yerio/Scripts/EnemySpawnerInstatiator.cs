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
        var networkCallback = GetComponent<NetworkCallbacks>();

        var reference = BoltNetwork.Instantiate(enemySpawner, Vector3.zero, Quaternion.identity);
        var enemySpawning = reference.GetComponent<EnemySpawningHandler>();
        networkCallback.enemySpawning = enemySpawning;
        enemySpawning.targetPoints = targetPoints;
        enemySpawning.spawnPoints = spawnPoints;

        
       var reference2 = BoltNetwork.Instantiate(gateHealth, instatiatePoint.position, Quaternion.identity);
       var gateHealthRef = reference2.GetComponent<GateHealth>();
       networkCallback.gateHealth = gateHealthRef;
    }
}
