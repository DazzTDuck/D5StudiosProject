using Bolt;
using System.Collections;
using UdpKit;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkCallbacks : GlobalEventListener
{
    [SerializeField] GameObject cameraPrefab;
    [SerializeField] Transform[] spawnPoints;

    [HideInInspector] public EnemySpawningHandler enemySpawning;
    [HideInInspector] public GateHealth gateHealth;

    [SerializeField] GameInfo gameInfo;

    //public static NetworkCallbacks instance;

    //private void Awake()
    //{
    //    if (instance == null)
    //        instance = this;
    //    else
    //    {
    //        Destroy(gameObject);
    //        return;
    //    }

    //    DontDestroyOnLoad(gameObject);
    //}

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

        if (!player.GetComponentInChildren<PlayerController>().GetIfHost())
        {
            StartCoroutine(CheckName(player));
        }

        foreach (var connection in BoltNetwork.Connections)
        {
            if (player.IsOwner && !player.GetComponentInChildren<PlayerController>().GetIfHost())
            {
                player.GetComponentInChildren<PlayerController>().SetConnectionID(connection.ConnectionId.ToString());
            }
        }

        //enemySpawning.SetPlayer(player.GetComponentInChildren<PlayerController>());
        //gateHealth.SetPlayer(player.GetComponentInChildren<PlayerController>());
        gameInfo.SetPlayer(player.GetComponentInChildren<PlayerController>());

        //Debug.LogWarning("Setting player");
        //Debug.LogWarning(player.GetComponentInChildren<PlayerController>());
    }

    IEnumerator CheckName(BoltEntity entity)
    {
        yield return new WaitUntil(() => entity.GetComponentInChildren<WaitForHostScreen>().CheckFirstPlayer());

        if (entity.IsOwner)
        {
            entity.GetComponentInChildren<PlayerController>().SetHost();
            entity.GetComponentInChildren<WaitForHostScreen>().SendChangeNameRequest("PlayerHost");
        }
    }

    public override void OnEvent(TeamKillEvent evnt)
    {
        if(evnt.TeamKillString == "Team1") { gameInfo.AddTeam1Kill(); }
        else if(evnt.TeamKillString == "Team2") { gameInfo.AddTeam2Kill(); }
    }

    public override void OnEvent(DestroyRequest evnt)
    {
        if (evnt.Entity.IsOwner)
        {
            var player = evnt.Entity.gameObject;
            player.GetComponentInChildren<PlayerController>().gameObject.transform.position = GetNewSpawnpoint();
            player.GetComponentInChildren<Rigidbody>().velocity = Vector3.zero;
            player.GetComponentInChildren<Rigidbody>().useGravity = true;

            if(player.GetComponentInChildren<Shoot>()) { player.GetComponentInChildren<Shoot>().ResetAmmo(); }
            if(player.GetComponentInChildren<Scout>()) 
            { 
                player.GetComponentInChildren<Scout>().ResetAmmo();
                player.GetComponentInChildren<GrapplingHook>().StopGrapple(false);
            }

            player.GetComponentInChildren<AbilityHandler>().ResetTimers();
            player.GetComponentInChildren<Health>().ResetHealth();

            if (!evnt.KillTrigger) { player.GetComponentInChildren<Health>().AddTeamKill(); }
        }
    }

    public override void OnEvent(DamageRequest evnt)
    {
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

    public override void OnEvent(StunEvent evnt)
    {
            evnt.EntityShot.GetComponentInChildren<PlayerController>().StartStun(evnt.Duration);
    }

    public override void OnEvent(HealRequest evnt)
    {
        evnt.EntityShot.GetComponentInChildren<Health>().GetHealing(evnt.Healing, evnt.HealStim);
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


