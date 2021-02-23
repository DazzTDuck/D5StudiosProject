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
            var reference = BoltNetwork.Instantiate(gameObject);
        }
    }
}
