using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bolt;

public class NetworkCallbacks : GlobalEventListener
{
    [SerializeField] GameObject playerPrefab;

    public override void SceneLoadLocalDone(string scene)
    {
        var spawnPos = new Vector3(Random.Range(-5, 5), 2, Random.Range(-5, 5));
        BoltNetwork.Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }
}


