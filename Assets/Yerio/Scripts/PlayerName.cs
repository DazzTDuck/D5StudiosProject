using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerName : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] TMP_InputField playerInputName;
    [SerializeField] Button button;
    [SerializeField] GameObject setNameCanvas;

    string playerNameLocal;

    public override void Attached()
    {
        //state.PlayerName = playerNameLocal;
        state.AddCallback("PlayerName", ChangeNameCallback);
        state.IsDead = true;
    }
    private void Start()
    {
        button.onClick.AddListener(() => { SendChangeNameRequest(playerInputName.text); });
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
        var request2 = DestroyRequest.Create();
        request2.Entity = GetComponentInParent<BoltEntity>();
        request2.IsPlayer = true;
        request2.Send();
    }

    public void ChangeName(string name)
    {
        state.PlayerName = name.Length > 2 ? name : "Player";
        playerNameText.text = state.PlayerName;
        //state.SetDynamic("PlayerName", state.PlayerName);

        PlayerCamera.HideCursor();
        setNameCanvas.SetActive(false);
    }
}
