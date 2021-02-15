using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJoined : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] GameObject[] objectsToSetActiveForPlayer;

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
