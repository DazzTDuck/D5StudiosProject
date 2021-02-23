using Bolt;
using UdpKit;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkCallbacks : GlobalEventListener
{
    [SerializeField] GameObject cameraPrefab;
    [SerializeField] Transform[] spawnPoints;

    [HideInInspector] public EnemySpawningHandler enemySpawning;
    [HideInInspector] public GateHealth gateHealth;

    GameInfo gameInfo;

    static UdpEndPoint localEndpoint;

    private void Update()
    {
        if (!gameInfo) { gameInfo = FindObjectOfType<GameInfo>(); }
    }

    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        base.SceneLoadLocalDone(scene, token);
        var player = BoltNetwork.Instantiate(cameraPrefab, GetNewSpawnpoint(), Quaternion.identity);
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
            if (player.IsOwner && localEndpoint == null)
            {
                localEndpoint = connection.RemoteEndPoint;
            }

            if (player.IsOwner && localEndpoint == connection.RemoteEndPoint)
            {
                Debug.LogWarning("ConnectionId");
                player.GetComponentInChildren<PlayerController>().SetConnectionID(connection.ConnectionId.ToString());
            }
        }

        if (player.GetComponentInChildren<PlayerController>().state.ConnectionID == null)
        {
            if (player.IsOwner)
            {
                localEndpoint = UdpEndPoint.Any;
                player.GetComponentInChildren<PlayerController>().SetHost();
            }         
        }

        //enemySpawning.SetPlayer(player.GetComponentInChildren<PlayerController>());
        //gateHealth.SetPlayer(player.GetComponentInChildren<PlayerController>());
        gameInfo.SetPlayer(player.GetComponentInChildren<PlayerController>());
    }

    public override void OnEvent(TeamKillEvent evnt)
    {
        if(evnt.TeamKillString == "Team1") { gameInfo.AddTeam1Kill(); }
        else if(evnt.TeamKillString == "Team2") { gameInfo.AddTeam2Kill(); }
    }

    public override void OnEvent(DestroyRequest evnt)
    {
        if (evnt.IsPlayer)
        {
            var player = evnt.Entity.gameObject;
            player.GetComponentInChildren<PlayerController>().gameObject.transform.position = GetNewSpawnpoint();
            player.GetComponentInChildren<Rigidbody>().velocity = Vector3.zero;
            player.GetComponentInChildren<Rigidbody>().useGravity = true;

            if(player.GetComponentInChildren<Shoot>()) { player.GetComponentInChildren<Shoot>().ResetAmmo(); }
            if(player.GetComponentInChildren<Shotgun>()) { player.GetComponentInChildren<Shotgun>().ResetAmmo(); }

            player.GetComponentInChildren<AbilityHandler>().ResetTimers();
            player.GetComponentInChildren<Health>().ResetHealth();
            return;
        }

        if (evnt.IsEnemy && evnt.Entity)
            enemySpawning.RemoveEnemyFromList(evnt.Entity.gameObject);

        if (evnt.Entity)
            BoltNetwork.Destroy(evnt.Entity.gameObject);
    }

    public override void OnEvent(DamageRequest evnt)
    {
        if (evnt.IsEnemy)
        {
            if (evnt.EntityShot.IsOwner)
                evnt.EntityShot.GetComponent<EnemyHealth>().TakeDamage(evnt.Damage);
            return;
        }

        if (evnt.EntityShot)
        {
            evnt.EntityShot.GetComponentInChildren<Health>().TakeDamage(evnt.Damage);
        }
    }

    public override void OnEvent(TeamTagEvent evnt)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject player in players)
        {
            player.GetComponentInChildren<PlayerController>().SetTagForServer();
        }
    }

    public override void OnEvent(DisconnectEvent evnt)
    {
        if (!evnt.EnitityToDisconnect.IsOwner && evnt.DisconnectEveryone)
        {
            BoltLauncher.Shutdown();
            SceneManager.LoadScene(0);
        }

        if (evnt.EnitityToDisconnect.IsOwner && evnt.DisconnectEveryone)
        {          
            BoltLauncher.Shutdown();
            SceneManager.LoadScene(0);
            return;
        }

        if (evnt.EnitityToDisconnect.IsOwner && !evnt.DisconnectEveryone)
        {
            //Debug.LogWarning("Disconnect person given");
            if (evnt.EnitityToDisconnect)
                evnt.EnitityToDisconnect.GetComponentInChildren<PauseMenuHandler>().Disconnect();
        }
    }

    public override void OnEvent(SendReadyRequest evnt)
    {
        foreach (var waitForHost in FindObjectsOfType<WaitForHostScreen>())
        {
            if (waitForHost.entity.IsOwner)
            {
                waitForHost.AddPlayerReady();
            }       
        }
    }

    public override void OnEvent(RestartRequest evnt)
    {
        BoltNetwork.LoadScene(SceneManager.GetActiveScene().name);
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

        if (evnt.Entity.IsOwner)
            gameInfo.SetGameStarted();

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


