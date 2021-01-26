using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCopy : Bolt.EntityBehaviour<IPlayerCameraState>
{
    [SerializeField] Transform cameraTransform;

    public override void Attached()
    {
        base.Attached();
        state.SetTransforms(state.CameraTransform, transform);
    }

    public override void SimulateOwner()
    {
        base.SimulateOwner();
        
        if (cameraTransform)
        {
            transform.position = cameraTransform.position;
            transform.rotation = cameraTransform.rotation;
        }
    }
}
