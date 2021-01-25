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

    public override void Attached()
    {
        state.Name = localName;       
    }
    private void Start()
    {
        int number = BoltMatchmaking.CurrentSession.ConnectionsCurrent;

        nameText.text = state.Name + " " + number.ToString();
    }

}
