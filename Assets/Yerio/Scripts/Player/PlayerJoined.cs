using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJoined : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] GameObject[] objectsToSetActiveForPlayer;
    [SerializeField] GameObject[] objectsToSetActiveForPlayerOnAttached;
    [SerializeField] GameObject[] objectsToSetNotActiveForPlayer;
    [SerializeField] GameObject[] objectsToHideFromYourCamera;

    public override void Attached()
    {
        foreach (var gameObject in objectsToSetActiveForPlayerOnAttached)
        {
            if (entity.IsOwner && !gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }
        }

        foreach (var gameObject in objectsToSetNotActiveForPlayer)
        {
            if (entity.IsOwner && gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
            }
        }

        foreach (var gameObject in objectsToHideFromYourCamera)
        {
            if (entity.IsOwner && gameObject.layer != 13)
            {
                gameObject.layer = 13;
            }
        }
    }

    private void Update()
    {
        foreach (var gameObject in objectsToSetActiveForPlayer)
        {
            if (entity.IsOwner && !gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }
        }
    }
}
