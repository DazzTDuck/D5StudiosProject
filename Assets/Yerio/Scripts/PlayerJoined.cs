using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerJoined : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] Camera playerCamera;

    public override void Detached()
    {
        base.Detached();
        //SceneManager.LoadScene(0);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if(entity.IsOwner && !playerCamera.gameObject.activeInHierarchy)
        {
            playerCamera.gameObject.SetActive(true);
        }
    }
}
