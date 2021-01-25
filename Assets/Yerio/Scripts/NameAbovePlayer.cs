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

    public void Start()
    {
        int number = BoltMatchmaking.CurrentSession.ConnectionsCurrent;
        nameText.text = $"Player {number}";
    }
}
