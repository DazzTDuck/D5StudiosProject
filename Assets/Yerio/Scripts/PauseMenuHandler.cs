using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bolt;

public class PauseMenuHandler : Bolt.EntityBehaviour<IPlayerControllerState>
{
    bool isPaused;
    Canvas canvas;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    public void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            OpenAndClosePauseMenu();
        }
    }

    public void OpenAndClosePauseMenu()
    {
        isPaused = !isPaused;
        canvas.enabled = isPaused;

        if (isPaused) { PlayerCamera.ShowCursor(); } else { PlayerCamera.HideCursor(); }    
    }

    public bool GetIfPaused() { return isPaused; }

    public void OpenSettings()
    {

    }
    public void CloseSettings()
    {

    }

    public void Disconnect()
    {
        int connections = 0;

        foreach (var connection in BoltNetwork.Connections)
        {
            connections++;

            if (connection.ConnectionId.ToString() == state.ConnectionID)
            {
                connection.Disconnect();
                BoltLauncher.Shutdown();
                SceneManager.LoadScene(0);
            }
            else if(state.ConnectionID == null)
            {
                foreach (var connection2 in BoltNetwork.Connections)
                {
                    connection2.Disconnect();
                }
                BoltLauncher.Shutdown();
                SceneManager.LoadScene(0);
            }
        }

        if(connections == 0)
        {
            BoltLauncher.Shutdown();
            SceneManager.LoadScene(0);
        }
    }
}
