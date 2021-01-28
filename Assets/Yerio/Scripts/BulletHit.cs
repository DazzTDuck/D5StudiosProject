using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHit : Bolt.EntityBehaviour<IBulletHitEffectState>
{
    public override void Attached()
    {
        state.SetTransforms(state.HitEffectTransform, transform);
    }
}
