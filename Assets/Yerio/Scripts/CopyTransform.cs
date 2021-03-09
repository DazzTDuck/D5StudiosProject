using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyTransform : MonoBehaviour
{
    [SerializeField] Transform toCopy;

    private void Update()
    {
        transform.position = toCopy.position;
        transform.rotation = toCopy.rotation;
    }
}
