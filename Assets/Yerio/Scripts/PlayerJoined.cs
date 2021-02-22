using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJoined : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] GameObject[] objectsToSetActiveForPlayer;
    [SerializeField] GameObject[] objectsToSetActiveForPlayerOnAttached;
    [SerializeField] GameObject[] objectsToSetNotActiveForPlayer;

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
