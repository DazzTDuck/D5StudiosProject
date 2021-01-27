using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : Bolt.EntityBehaviour<IEnemyState>
{
    public override void Attached()
    {
        state.SetTransforms(state.EnemyTransform, transform);
    }
}
