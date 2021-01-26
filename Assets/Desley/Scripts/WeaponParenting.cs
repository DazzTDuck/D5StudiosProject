using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponParenting : MonoBehaviour
{
    [SerializeField] Transform cam;

    void Update()
    {
        if(cam.gameObject.activeSelf == true)
        {
            transform.SetParent(cam);
        }
    }
}
