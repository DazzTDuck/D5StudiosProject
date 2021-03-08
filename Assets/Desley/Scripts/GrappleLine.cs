using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleLine : MonoBehaviour
{
    public Transform start;
    public Vector3 endPos;

    void LateUpdate()
    {
        var request = UpdateGrappleLine.Create();
        request.GrappleLine = GetComponent<BoltEntity>();
        request.StartPos = start.position;
        request.EndPos = endPos;
        request.Send();
    }
}
