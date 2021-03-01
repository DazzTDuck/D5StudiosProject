using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Bolt;
using Bolt.Matchmaking;
using UdpKit;
using TMPro;


public class MainMenu : GlobalEventListener
{
    [SerializeField] Button hostButton;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //host game
    public void StartServer()
    {
        BoltLauncher.StartServer();
    }

    public override void BoltStartDone()
    {
        base.BoltStartDone();

        BoltMatchmaking.CreateSession(sessionID: "Epic Game", sceneToLoad: "Yerio");
    }

    //join game button
    public void StartClient()
    {
        BoltLauncher.StartClient();
    }

    public void ShutdownBolt()
    {
        BoltLauncher.Shutdown();
    }

    public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
    {
        base.SessionListUpdated(sessionList);

        foreach (var session in sessionList)
        {
            UdpSession photonSession = session.Value as UdpSession;

            if (photonSession.Source == UdpSessionSource.Photon)
            {
                JoinSession(photonSession);
            }
        }
    }

    public void JoinSession(UdpSession session)
    {
        BoltMatchmaking.JoinSession(session);
    }
}
