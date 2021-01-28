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

        var spawnPos = new Vector3(Random.Range(-2, 2), 2, Random.Range(-2, 2));
        BoltNetwork.Instantiate(playerPrefab, token, spawnPos, Quaternion.identity);

        var spawnPos2 = new Vector3(Random.Range(-2, 2), 5, Random.Range(-2, 2));
        BoltNetwork.Instantiate(enemyPrefab, token, spawnPos2, Quaternion.identity);
    }

    public override void OnEvent(DestroyRequest evnt)
    {
        BoltNetwork.Destroy(evnt.Entity.gameObject);
    }

    public override void OnEvent(DamageRequest evnt)
    {
        evnt.Entity.GetComponentInChildren<Health>().TakeDamage(evnt.Damage);
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


