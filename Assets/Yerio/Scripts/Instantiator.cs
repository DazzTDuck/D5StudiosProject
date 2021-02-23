using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class Instantiator : MonoBehaviour
{
    [SerializeField] GameObject[] objectsToInstatiateOnNetwork;
    
    private void Start()
    {
        var networkCallback = GetComponent<NetworkCallbacks>();

        foreach (var gameObject in objectsToInstatiateOnNetwork)
        {
            if(gameObject.GetComponent<GameInfo>() && FindObjectOfType<GameInfo>())
            {
                continue;
            }
            var reference = BoltNetwork.Instantiate(gameObject);
        }
    }
}
