using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJoined : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] Camera playerCamera;
    [SerializeField] GameObject playerSetNameCanvas;

    public override void Attached()
    {
        if (entity.IsOwner && !playerSetNameCanvas.gameObject.activeInHierarchy)
        {
            playerSetNameCanvas.gameObject.SetActive(true);
        }
    }
    private void Update()
    {
        if (entity.IsOwner && !playerCamera.gameObject.activeInHierarchy)
        {
            playerCamera.gameObject.SetActive(true);
        }
    }
}
