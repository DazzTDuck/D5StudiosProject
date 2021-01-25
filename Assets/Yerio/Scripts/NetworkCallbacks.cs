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
        var spawnPos = new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
        BoltNetwork.Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }

    public override void Disconnected(BoltConnection connection)
    {
        base.Disconnected(connection);
        SceneManager.LoadScene(0);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }
}
