using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bolt;
using UdpKit;

public class NetworkCallbacks : GlobalEventListener
{
    [SerializeField] GameObject cameraPrefab;
    [SerializeField] Transform[] spawnPoints;
    public EnemySpawningHandler enemySpawning;
    //[SerializeField] GameObject enemyPrefab;

    [Space, SerializeField] int connectionsAmount;

    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        base.SceneLoadLocalDone(scene, token);
        var player = BoltNetwork.Instantiate(cameraPrefab, GetNewSpawnpoint(), Quaternion.identity);

        foreach (var connection in BoltNetwork.Connections)
        {
            connectionsAmount++;
        }

        //BoltNetwork.Instantiate(enemyPrefab, token, GetNewSpawnpoint() + Vector3.right * Random.Range(-3,3), Quaternion.identity);
    }

    public void SpawnPlayer(GameObject objectToRemove, GameObject newPrefab)
    {
        //Create DestroyRequest, set entity to ent and then send
        var request = DestroyRequest.Create();
        request.Entity = objectToRemove.GetComponent<BoltEntity>();
        request.Send();
        var player = BoltNetwork.Instantiate(newPrefab, GetNewSpawnpoint(), Quaternion.identity);

        if (connectionsAmount == 0)
        {
            player.GetComponentInChildren<PlayerController>().SetHost();
        }

        enemySpawning.SetPlayer(player.GetComponentInChildren<PlayerController>());
    }

    public override void OnEvent(DestroyRequest evnt)
    {
        if (evnt.IsPlayer)
        {
            var player = evnt.Entity.gameObject;
            player.GetComponentInChildren<PlayerController>().gameObject.transform.position = GetNewSpawnpoint();
            player.GetComponentInChildren<Rigidbody>().velocity = Vector3.zero;
            player.GetComponentInChildren<Rigidbody>().useGravity = true;
            player.GetComponentInChildren<Health>().ResetHealth();
            player.GetComponentInChildren<PlayerController>().GetComponentInChildren<CameraCopy>().GetComponentInChildren<Shoot>().ResetAmmo();
            return;
        }

        if (evnt.IsEnemy)
            enemySpawning.RemoveEnemyFromList(evnt.Entity.gameObject);

        BoltNetwork.Destroy(evnt.Entity.gameObject);
    }

    public override void OnEvent(DamageRequest evnt)
    {
        if (evnt.IsEnemy)
        {
            evnt.Entity.GetComponent<EnemyHealth>().TakeDamage(evnt.Damage);
            return;
        }
        evnt.Entity.GetComponentInChildren<Health>().TakeDamage(evnt.Damage);
        
    }

    public override void OnEvent(ChangeNameEvent evnt)
    {
        evnt.Entity.GetComponentInChildren<WaitForHostScreen>().ChangeName(evnt.Name);
    }

    public override void OnEvent(StartGameRequest evnt)
    {
        foreach (var host in FindObjectsOfType<WaitForHostScreen>())
        {
            if (host.entity.IsOwner)
            {
                host.CloseScreen();
            }              
        }

        if (evnt.GameStarted && evnt.Entity.IsOwner)
        {
            evnt.Entity.GetComponentInChildren<AbilityHandler>().ActivateAbilities();
        }
    }

    //fix later
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


