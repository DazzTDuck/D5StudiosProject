using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bolt;
using UdpKit;

public class NetworkCallbacks : GlobalEventListener
{
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject playerPrefab;

    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        base.SceneLoadLocalDone(scene, token);

        var spawnPos = new Vector3(Random.Range(-5, 5), 2, Random.Range(-5, 5));
        BoltNetwork.Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        BoltNetwork.Instantiate(enemyPrefab, new Vector3(Random.Range(-5, 5), 5, Random.Range(-5, 5)), Quaternion.identity);
    }

    public override void EntityDetached(BoltEntity entity)
    {
        base.EntityDetached(entity);

        if (entity.IsControlled)
        {
            if (BoltNetwork.Server.DisconnectReason == UdpKit.UdpConnectionDisconnectReason.Disconnected)
            {
                foreach (var client in BoltNetwork.Connections)
                {
                    client.Disconnect();
                }
                SceneManager.LoadScene(0);
            }
        }       
    }
}


