using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bolt;

public class PlayerLeave : GlobalEventListener
{
    uint connectionID;
    bool hasID = false;

    private void Update()
    {
        if (Input.GetButtonDown("LeaveGame"))
        {
            foreach (var connection in BoltNetwork.Connections)
            {
                if(connection.ConnectionId == connectionID)
                {
                    connection.Disconnect();
                    Debug.Log("Leave Game");
                }
            }
        }
    }
}
