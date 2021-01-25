using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bolt;
using Bolt.Matchmaking;

public class PlayerJoined : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] Camera playerCamera;

    private void Update()
    {
        if(entity.IsOwner && !playerCamera.gameObject.activeInHierarchy)
        {
            playerCamera.gameObject.SetActive(true);
        }

        if (!BoltNetwork.IsConnected && entity.IsAttached)
        {
            SceneManager.LoadScene(0);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
        }
    }
}
