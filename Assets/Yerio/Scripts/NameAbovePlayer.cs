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

    public override void SimulateOwner()
    {
        int number = BoltMatchmaking.CurrentSession.ConnectionsCurrent;

        if (nameText.text != $"Player {number}")
            nameText.text = $"Player {number}";
    }

}
