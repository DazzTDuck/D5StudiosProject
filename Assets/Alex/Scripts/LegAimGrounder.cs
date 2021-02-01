using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegAimGrounder : MonoBehaviour
{
    int layerMask; 
    public GameObject LegAimRaycastOrigin; 
    void Start()
    {
        layerMask = LayerMask.GetMask("Ground"); 
        LegAimRaycastOrigin = transform.parent.gameObject;
    }


    void Update()
    {
        RaycastHit hit; 
        if (Physics.Raycast(LegAimRaycastOrigin.transform.position, -transform.up, out hit, Mathf.Infinity, layerMask))
        {
            transform.position = hit.point + new Vector3(0f, 0.3f, 0f); 
        }

    }
}
