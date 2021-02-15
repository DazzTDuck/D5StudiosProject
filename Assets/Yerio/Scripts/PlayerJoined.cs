using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJoined : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] GameObject[] objectsToSetActiveForPlayer;
    [SerializeField] GameObject[] objectsToSetActiveForPlayerOnAttached;

    public override void Attached()
    {
        foreach (var gameObject in objectsToSetActiveForPlayerOnAttached)
        {
            if (entity.IsOwner && !gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
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
