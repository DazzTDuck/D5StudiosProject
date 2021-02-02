using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaitForHostScreen : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] TMP_InputField playerInputName;
    [SerializeField] Button button;
    [SerializeField] GameObject setNameCanvas;

    [SerializeField] TMP_Text[] playerNamesInLobby;
    [SerializeField] List<GameObject> players = new List<GameObject>();
 
    string playerNameLocal;

    public override void Attached()
    {
        if (entity.IsOwner)
            state.PlayerName = playerNameLocal;

        state.AddCallback("PlayerName", ChangeNameCallback);
        state.IsDead = true;
    }
    private void Start()
    {
        button.onClick.AddListener(() => { SendChangeNameRequest(playerInputName.text); });
    }

    public void StartGame()
    {
        //Disable all Host Screens (event)
        //start spawning enemies
    }

    public void GetAllPlayers()
    {
        //update if there are players in the scene
        //add them into the Players List
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

        //Create DestroyRequest, set entity to ent and then send
        //var request2 = DestroyRequest.Create();
        //request2.Entity = GetComponentInParent<BoltEntity>();
        //request2.IsPlayer = true;
        //request2.Send();
        state.IsDead = false;
    }

    public void ChangeName(string name)
    {
        state.PlayerName = name.Length > 2 ? name : "Player";
        playerNameText.text = state.PlayerName;
        //state.SetDynamic("PlayerName", state.PlayerName);

        PlayerCamera.HideCursor();
        setNameCanvas.SetActive(false);

        //change corresponding name in array to what has been set.
    }
}
