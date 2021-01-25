using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bolt;

public class PlayerLeave : GlobalEventListener
{
    public void Leave()
    {
        SceneManager.LoadScene(0);
    }

    public override void Disconnected(BoltConnection connection)
    {
        base.Disconnected(connection);
        connection.Disconnect();
        Leave();
    }
}
