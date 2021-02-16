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

        foreach (var connection in BoltNetwork.Connections)
        {
            if (player.IsOwner)
            {
                Debug.LogWarning("ConnectionId");
                player.GetComponentInChildren<PlayerController>().SetConnectionID(connection.ConnectionId.ToString());
            }
        }

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
            return;
        }

        if (evnt.IsEnemy && evnt.Entity)
            enemySpawning.RemoveEnemyFromList(evnt.Entity.gameObject);

        BoltNetwork.Destroy(evnt.Entity.gameObject);
    }

    public override void OnEvent(DamageRequest evnt)
    {
        if (evnt.IsEnemy)
        {
            evnt.EntityShot.GetComponent<EnemyHealth>().TakeDamage(evnt.Damage);
            return;
        }

        if (evnt.EntityShot)
            evnt.EntityShot.GetComponentInChildren<Health>().TakeDamage(evnt.Damage);
    }

    public override void OnEvent(DisconnectEvent evnt)
    {
        if (evnt.DisconnectEveryone && evnt.EnitityToDisconnect.IsOwner)
        {
            foreach (var connection in BoltNetwork.Connections)
            {
                connection.Disconnect();             
            }
            BoltLauncher.Shutdown();
            SceneManager.LoadScene(0);
        }

        if (evnt.EnitityToDisconnect)
        {
            evnt.EnitityToDisconnect.GetComponentInChildren<PauseMenuHandler>().Disconnect();
        }
    }

    public override void OnEvent(HealRequest evnt)
    {
        evnt.EntityShot.GetComponentInChildren<Health>().GetHealing(evnt.Healing);
    }

    public override void OnEvent(ChangeNameEvent evnt)
    {
        evnt.Entity.GetComponentInChildren<WaitForHostScreen>().ChangeName(evnt.Name);
    }

    public override void OnEvent(StartGameRequest evnt)
    {
        if (evnt.GameStarted && evnt.Entity.IsOwner)
        {
            evnt.Entity.GetComponentInChildren<AbilityHandler>().ActivateAbilities();
            return;
        }

        foreach (var host in FindObjectsOfType<WaitForHostScreen>())
        {
            if (host.entity.IsOwner)
            {
                host.CloseScreen();
                host.entity.GetComponentInChildren<AbilityHandler>().ActivateAbilities();
            }              
        }
    }
   
    public Vector3 GetNewSpawnpoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)].position;
    }
}


