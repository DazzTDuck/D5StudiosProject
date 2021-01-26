﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCopy : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform weaponTransfrom;

    public override void Attached()
    {
        base.Attached();
        state.SetTransforms(state.GunTransform, weaponTransfrom);
    }

    public override void SimulateOwner()
    {
        if (cameraTransform)
        {
            transform.position = Vector3.Slerp(transform.position, cameraTransform.position, BoltNetwork.FrameDeltaTime * 15);
            transform.rotation = Quaternion.Slerp(transform.rotation, cameraTransform.rotation, BoltNetwork.FrameDeltaTime * 15);
        }
    }
}
