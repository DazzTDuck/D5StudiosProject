using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformUpdateFakeCamera : Bolt.EntityBehaviour<IPlayerControllerState>
{
    public override void Attached()
    {
        state.SetTransforms(state.GunTransform, transform);
    }

}

