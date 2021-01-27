using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Bolt.EntityBehaviour<IBulletState>
{
    [SerializeField] float bulletSpeed;
    Vector3 dir;

    public void SetDirection(Vector3 direction)
    {
        dir = direction;
    }

    public override void Attached()
    {
        base.Attached();
        state.SetTransforms(state.BulletTransform, transform);
    }

    public override void SimulateOwner()
    {
        base.SimulateOwner();
        transform.Translate(dir * bulletSpeed * BoltNetwork.FrameDeltaTime);
    }
}
