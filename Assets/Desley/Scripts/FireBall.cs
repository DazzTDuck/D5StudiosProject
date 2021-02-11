using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : Bolt.EntityBehaviour<IFireBallState>
{


    public override void Attached()
    {
        base.Attached();
        state.SetTransforms(state.FireBallTransform, transform);
    }

    private void OnCollisionEnter(Collision collision)
    {
            //Create DestroyRequest, set entity to ent and then send
            var request = DestroyRequest.Create();
            request.Entity = GetComponent<BoltEntity>();
            request.IsEnemy = false;
            request.Send();
    }
}
