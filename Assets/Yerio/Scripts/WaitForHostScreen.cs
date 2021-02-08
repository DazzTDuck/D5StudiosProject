using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaitForHostScreen : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [Header("Player Name")]
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] TMP_InputField playerInputName;
    [SerializeField] Button setNameButton;

    [Space, SerializeField] GameObject startGameCanvas;

    [SerializeField] Button startGameButton;

    [SerializeField] TMP_Text[] playerNamesInLobby;
    [SerializeField] List<GameObject> players = new List<GameObject>();

    string playerNameLocal = "Player";

    public override void Attached()
    {
        if (entity.IsOwner)
            state.PlayerName = playerNameLocal;

        state.AddCallback("PlayerName", ChangeNameCallback);
        state.IsDead = true;

        startGameButton.interactable = false;
        StartCoroutine(StartButtonInteractable());
    }
    private void Start()
    {
        setNameButton.onClick.AddListener(() => { SendChangeNameRequest(playerInputName.text); CheckIfGameStarted(); });
        startGameButton.onClick.AddListener(() => { StartGame(); });
    }
    private void Update()
    {
        if (startGameCanvas.activeSelf)
            GetAllPlayers();
    }

    public void StartGame()
    {
        //Disable all Host Screens (event)
        var request = StartGameRequest.Create();
        request.Entity = entity;
        request.Send();

        //start spawning enemies
        EnemySpawningHandler.StartGame();
    }

    public void CloseScreen()
    {
        startGameCanvas.SetActive(false);
        PlayerCamera.HideCursor();
        state.IsDead = false;
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
        //Send Event To update Host Screen to corrensponding Player name      
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
        foreach (var started in FindObjectsOfType<EnemySpawningHandler>())
        {
            if (started.GetIfGameStarted())
            {
                CloseScreen();
                var request = StartGameRequest.Create();
                request.Entity = entity;
                request.GameStarted = true;
                request.Send();
            }
        }
    }

    IEnumerator StartButtonInteractable()
    {
        yield return new WaitForSeconds(4);

        bool host = GetComponent<PlayerController>().GetIfHost();
        startGameButton.interactable = host;
    }
}
