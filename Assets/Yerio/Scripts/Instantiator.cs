using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class Instantiator : MonoBehaviour
{
    [SerializeField] GameObject[] objectsToInstatiateOnNetwork;
    
    private void Start()
    {
        foreach (var gameObject in objectsToInstatiateOnNetwork)
        {
            BoltNetwork.Instantiate(gameObject);
        }
    }
}
