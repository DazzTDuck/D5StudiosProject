using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bolt;

public class PauseMenuHandler : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] GameObject panel;
    bool isPaused;
    bool inSettings = false;

    GameInfo gameInfo;

    public void Update()
    {
        if(!gameInfo) { gameInfo = FindObjectOfType<GameInfo>(); }

        if (Input.GetButtonDown("Cancel") && gameInfo.state.GameStarted && !inSettings)
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
        inSettings = true;
    }
    public void CloseSettings()
    {
        inSettings = false;
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
