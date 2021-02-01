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

    int posIndex;

    public override void Attached()
    {
        state.SetTransforms(state.EnemyTransform, transform);
    }

    private void Update()
    {
        AttackTarget();
    }
    public void AttackTarget()
    {
        if (GetTargetDistance() <= attackingDistance)
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
        target = targetPos;
        agent.SetDestination(target);
        agent.speed = moveSpeed * 10 * BoltNetwork.FrameDeltaTime;
    }
}
