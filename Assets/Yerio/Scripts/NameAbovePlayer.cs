using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Bolt;
using Bolt.Matchmaking;
using UdpKit;

public class NameAbovePlayer : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] TMP_Text nameText;

    readonly string localName = "Player";
    int playerNumber;

    public override void Attached()
    {
        state.Name = localName;
        state.PlayerNumber = playerNumber;
    }
    private void Start()
    {
        state.PlayerNumber = BoltMatchmaking.CurrentSession.ConnectionsCurrent;

        nameText.text = state.Name + " " + state.PlayerNumber.ToString();
    }

}
