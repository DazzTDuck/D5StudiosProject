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
        state.IsDead = true;
        crosshairCanvas.SetActive(false);
    }

    public void ChangeNameCallback()
    {
        playerNameLocal = state.PlayerName;
        playerNameText.text = state.PlayerName;
        state.IsDead = false;
        PlayerCamera.HideCursor();
        crosshairCanvas.SetActive(true);
        setNameCanvas.SetActive(false);
    }

    public void ChangeName()
    {
        var name = "";
        if(playerInputName.text.Length < 2) { name = "Player"; } else { name = playerInputName.text; }
        state.PlayerName = name;
    }
}
