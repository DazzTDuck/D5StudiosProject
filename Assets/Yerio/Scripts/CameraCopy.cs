using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCopy : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;

    public void Update()
    {
        if (cameraTransform)
        {
            transform.position = cameraTransform.position;
            transform.rotation = cameraTransform.rotation;
        }
    }
}
