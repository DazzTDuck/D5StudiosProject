using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class WaitForHostScreen : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [Header("Player Name")]
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] TMP_InputField playerInputName;
    [SerializeField] Button setNameButton;

    [Space, SerializeField] GameObject startGameCanvas;

    [SerializeField] Button startGameButton;

    [SerializeField] TMP_Text[] playerNamesInLobby;
    public List<GameObject> players = new List<GameObject>();

    string playerNameLocal = "Player";
    GameInfo gameInfo;

    [SerializeField] List<bool> playersReady = new List<bool>();
    [SerializeField] bool allPlayersReady = false;

    bool host;

    public override void Attached()
    {
        if (entity.IsOwner)
            state.PlayerName = playerNameLocal;

        state.AddCallback("PlayerName", ChangeNameCallback);
        state.IsDead = true;

        startGameButton.interactable = false;
        StartCoroutine(GetHost());
    }
    private void Start()
    {
        setNameButton.onClick.AddListener(() => { SendChangeNameRequest(playerInputName.text); });
        startGameButton.onClick.AddListener(() => { StartGame(); });
    }
    private void Update()
    {
        if(!gameInfo) { gameInfo = FindObjectOfType<GameInfo>(); }

        
        if (startGameCanvas.activeSelf)
        {
            GetAllPlayers();
            GetIfAllPlayersReady();

            startGameButton.interactable = host && allPlayersReady;

            if (gameInfo)
            {
                CheckIfGameStarted();
            }
        }
    }

    public void StartGame()
    {
        //Disable all Host Screens (event)
        var request = StartGameRequest.Create();
        request.Entity = entity;
        request.Send();

        //start spawning enemies
        //EnemySpawningHandler.StartGame();
    }

    public void CloseScreen()
    {
        startGameCanvas.SetActive(false);
        PlayerCamera.HideCursor();
        state.IsDead = false;
    }

    public void RespawnPlayer()
    {
        GetComponent<Health>().RespawnPlayer();
    }

    public void GetAllPlayers()
    {
        //update if there are players in the scene
        //add them into the Players List
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (!players.Contains(player))
            {
                players.Add(player);
            }
        }

        if(players.Count > 0)
        {
            for (int i = 0; i < players.Count; i++)
            {
                playerNamesInLobby[i].gameObject.SetActive(true);
                if (players[i].GetComponentInChildren<PlayerController>().GetIfHost())
                {
                    playerNamesInLobby[i].text = players[i].GetComponentInChildren<WaitForHostScreen>().playerNameText.text + " " + "(Host)";
                }
                else
                {
                    playerNamesInLobby[i].text = players[i].GetComponentInChildren<WaitForHostScreen>().playerNameText.text;
                }             
            }
        }      
    }

    public bool CheckFirstPlayer()
    {
        if(players.Count > 0)
        {
            if (players[0].GetComponentInChildren<PlayerController>().state.PlayerName == playerNameLocal) { return true; } else { return false; }
        }

        //Debug.LogWarning(players.Count);
        //Debug.LogWarning(players[0].GetComponentInChildren<PlayerController>().state.PlayerName);
        return false;     
    }

    public void ReadyRequest()
    {
        var request = SendReadyRequest.Create();
        request.Send();
    }

    public void AddPlayerReady() { playersReady.Add(true); }

    public void GetIfAllPlayersReady()
    {
        if(playersReady.Count == players.Count) { allPlayersReady = true; } else { allPlayersReady = false; }        
    }

    public void ChangeNameCallback()
    {
        playerNameLocal = state.PlayerName;
        playerNameText.text = state.PlayerName;
    }

    public void SendChangeNameRequest(string name)
    {
        var request = ChangeNameEvent.Create();
        request.Entity = GetComponentInParent<BoltEntity>();
        request.Name = name;
        request.Send();
    }

    public void ChangeName(string name)
    {
        if(entity.IsOwner)
        state.PlayerName = name.Length > 2 || name.Length < 20 ? name : "Player";

        playerNameText.text = state.PlayerName;
        //state.SetDynamic("PlayerName", state.PlayerName);
    }

    public void CheckIfGameStarted()
    {
        if (gameInfo.state.GameStarted)
        {
            var request = DisconnectEvent.Create();
            request.EnitityToDisconnect = entity;
            request.DisconnectEveryone = false;
            request.Send();
        }
    }

    IEnumerator GetHost()
    {
        yield return new WaitForSeconds(3);

        host = GetComponent<PlayerController>().GetIfHost();
    }
}
