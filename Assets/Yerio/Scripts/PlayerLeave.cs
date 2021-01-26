using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class PlayerLeave : GlobalEventListener
{
    public override void Disconnected(BoltConnection connection)
    {
        base.Disconnected(connection);
        connection.Disconnect();
        
    }
}
