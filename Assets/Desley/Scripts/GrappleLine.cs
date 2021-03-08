using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleLine : MonoBehaviour
{
    LineRenderer lr;
    public Transform start;
    public Vector3 endPos;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    private void LateUpdate()
    {
        lr.SetPosition(0, start.position);
        lr.SetPosition(1, endPos);
    }
}
