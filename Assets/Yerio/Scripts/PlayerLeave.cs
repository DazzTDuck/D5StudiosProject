using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bolt;

[BoltGlobalBehaviour(BoltNetworkModes.Client)]
public class PlayerLeave : GlobalEventListener
{
    public override void Disconnected(BoltConnection connection)
    {
        base.Disconnected(connection);
        connection.Disconnect();
        SceneManager.LoadScene(0);
    }
}
