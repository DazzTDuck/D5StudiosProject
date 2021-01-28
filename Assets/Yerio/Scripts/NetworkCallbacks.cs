using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bolt;
using UdpKit;

public class NetworkCallbacks : GlobalEventListener
{
    [SerializeField] GameObject playerPrefab;

    [SerializeField] Transform[] spawnPoints;

    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        base.SceneLoadLocalDone(scene, token);

        BoltNetwork.Instantiate(playerPrefab, token, GetNewSpawnpoint(), Quaternion.identity);
    }

    public override void OnEvent(DestroyRequest evnt)
    {
        if (evnt.IsPlayer)
        {
            var playerRef = evnt.Entity.gameObject;
            BoltNetwork.Instantiate(playerRef, GetNewSpawnpoint(), Quaternion.identity);
        }
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

    public Vector3 GetNewSpawnpoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)].position;
    }
}


