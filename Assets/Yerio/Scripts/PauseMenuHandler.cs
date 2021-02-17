﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bolt;

public class PauseMenuHandler : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] GameObject panel;
    bool isPaused;

    NetworkCallbacks networkCallbacks;

    public void Update()
    {
        if(!networkCallbacks) { networkCallbacks = FindObjectOfType<NetworkCallbacks>(); }

        if (Input.GetButtonDown("Cancel") && networkCallbacks.GetIfGameStarted())
        {
            OpenAndClosePauseMenu();
        }
    }

    public void OpenAndClosePauseMenu()
    {
        isPaused = !isPaused;
        panel.SetActive(isPaused);

        if (isPaused) { PlayerCamera.ShowCursor(); } else { PlayerCamera.HideCursor(); }    
    }

    public bool GetIfPaused() { return isPaused; }

    public void OpenSettings()
    {

    }
    public void CloseSettings()
    {

    }

    public void DisconnectEventSend()
    {
        var request = DisconnectEvent.Create();
        request.EnitityToDisconnect = entity;

        if(state.ConnectionID == null)
        {
            request.DisconnectEveryone = true;
        }
        else
        {
            request.DisconnectEveryone = false;
        }
        request.Send();
    }

    public void Disconnect()
    {
        int connections = 0;

        foreach (var connection in BoltNetwork.Connections)
        {
            connections++;

            if (connection.ConnectionId.ToString() == state.ConnectionID)
            {
                BoltLauncher.Shutdown();
                SceneManager.LoadScene(0);
            }           
        }
    }
}
