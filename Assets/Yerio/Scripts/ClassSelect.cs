using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassSelect : MonoBehaviour
{
    NetworkCallbacks networkCallbacks;
    [SerializeField] GameObject[] playerPrefabs;

    private void Start()
    {
        networkCallbacks = FindObjectOfType<NetworkCallbacks>();        
    }

    public void SelectClass(int buttonIndex)
    {
        switch (buttonIndex)
        {
            case 0:
                //gun prefab
                networkCallbacks.SpawnPlayer(transform.parent.gameObject, playerPrefabs[0]);
                break;
            case 1:
                //melee prefab
                networkCallbacks.SpawnPlayer(transform.parent.gameObject, playerPrefabs[1]);
                break;
            case 2:
                //other
                networkCallbacks.SpawnPlayer(transform.parent.gameObject, playerPrefabs[2]);
                break;
            case 3:
                //other
                networkCallbacks.SpawnPlayer(transform.parent.gameObject, playerPrefabs[3]);
                break;
        }
    }
}
