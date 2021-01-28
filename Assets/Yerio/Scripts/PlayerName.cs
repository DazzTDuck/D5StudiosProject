using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerName : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] TMP_InputField playerInputName;
    [SerializeField] GameObject setNameCanvas;
    [SerializeField] GameObject crosshairCanvas;

    string playerNameLocal;

    public override void Attached()
    {
        state.PlayerName = playerNameLocal;
        state.AddCallback("PlayerName", ChangeNameCallback);
        crosshairCanvas.SetActive(false);
        state.IsDead = true;
        GetComponentInParent<MeshRenderer>().enabled = false;
        GetComponentInParent<Collider>().enabled = false;
        GetComponentInParent<Rigidbody>().useGravity = false;
    }

    public void ChangeNameCallback()
    {
        playerNameLocal = state.PlayerName;
        playerNameText.text = state.PlayerName;

        //Create DestroyRequest, set entity to ent and then send
        var request = DestroyRequest.Create();
        request.Entity = GetComponentInParent<BoltEntity>();
        request.IsPlayer = true;
        request.Send();
    }

    public void ChangeName()
    {
        var name = "";
        if(playerInputName.text.Length < 2) { name = "Player"; } else { name = playerInputName.text; }
        state.PlayerName = name;
        playerNameText.text = state.PlayerName;
        state.SetDynamic("PlayerName", state.PlayerName);

        PlayerCamera.HideCursor();
        crosshairCanvas.SetActive(true);
        setNameCanvas.SetActive(false);
    }
}
