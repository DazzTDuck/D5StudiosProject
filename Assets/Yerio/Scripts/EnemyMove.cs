using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMove : Bolt.EntityBehaviour<IEnemyState>
{
    [SerializeField] float attackingDistance = 0.25f;
    [SerializeField] float moveSpeed = 20f;
    NavMeshAgent agent;
    Vector3 target = Vector3.zero;

    public override void Attached()
    {
        if (entity.IsOwner)
            state.SetTransforms(state.EnemyTransform, transform);
    }

    public override void SimulateOwner()
    {
        AttackTarget();
    }
    public void AttackTarget()
    {
        if (GetTargetDistance() <= attackingDistance && entity.IsOwner)
        {
            agent.isStopped = true;
            //attacking code after
        }
    }

    public float GetTargetDistance()
    {
        return Vector3.Distance(transform.position, target);
    }

    public void SetAgent(NavMeshAgent agent)
    {
        this.agent = agent;
    }

    public void SetPath(Vector3 targetPos)
    {
        if (entity.IsOwner)
        {
            target = targetPos;
            agent.SetDestination(target);
            agent.speed = moveSpeed * 10 * BoltNetwork.FrameDeltaTime;

            Debug.LogWarning("Desleyo very cool");
        }
    }
}
