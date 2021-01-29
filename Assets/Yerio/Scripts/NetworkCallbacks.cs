﻿using System.Collections;
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
            Debug.LogWarning("dead");
            var player = evnt.Entity.gameObject;
            player.GetComponentInChildren<PlayerController>().gameObject.transform.position = GetNewSpawnpoint();
            player.GetComponentInChildren<Rigidbody>().velocity = Vector3.zero;
            player.GetComponentInChildren<Rigidbody>().useGravity = true;
            player.GetComponentInChildren<Health>().ResetHealth();
            return;
        }
        BoltNetwork.Destroy(evnt.Entity.gameObject);
    }

    public override void OnEvent(DamageRequest evnt)
    {
        evnt.Entity.GetComponentInChildren<Health>().TakeDamage(evnt.Damage);
        Debug.LogWarning("shot");
    }

    public override void OnEvent(ChangeNameEvent evnt)
    {
        evnt.Entity.GetComponentInChildren<PlayerName>().ChangeName(evnt.Name);
    }

    public override void EntityDetached(BoltEntity entity)
    {
        base.EntityDetached(entity);

        if (entity.IsOwner && entity.IsControlled)
        {
            if (BoltNetwork.Server.DisconnectReason == UdpConnectionDisconnectReason.Timeout ||
                BoltNetwork.Server.DisconnectReason == UdpConnectionDisconnectReason.Disconnected ||
                BoltNetwork.Server.DisconnectReason == UdpConnectionDisconnectReason.Authentication ||
                BoltNetwork.Server.DisconnectReason == UdpConnectionDisconnectReason.Error ||
                BoltNetwork.Server.DisconnectReason == UdpConnectionDisconnectReason.Unknown)
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


