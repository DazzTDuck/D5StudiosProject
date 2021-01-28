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
    [SerializeField] GameObject crosshairCanvas;

    string playerNameLocal;

    public override void Attached()
    {
        state.PlayerName = playerNameLocal;
        state.AddCallback("PlayerName", ChangeNameCallback);
        crosshairCanvas.SetActive(false);
        state.IsDead = true;
    }
    private void Start()
    {
        button.onClick.AddListener(() => { ChangeName(playerInputName.text); });
    }

    public void ChangeNameCallback()
    {
        playerNameLocal = state.PlayerName;
        playerNameText.text = state.PlayerName;
        state.SetDynamic("PlayerName", state.PlayerName);

        //Create DestroyRequest, set entity to ent and then send
        var request = DestroyRequest.Create();
        request.Entity = GetComponentInParent<BoltEntity>();
        request.IsPlayer = true;
        request.Send();
    }

    public void ChangeName(string name)
    {
        state.PlayerName = name.Length > 2 ? name : "Playerz";
        playerNameText.text = state.PlayerName;
        state.SetDynamic("PlayerName", state.PlayerName);

        PlayerCamera.HideCursor();
        crosshairCanvas.SetActive(true);
        setNameCanvas.SetActive(false);
    }
}
