using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMove : Bolt.EntityBehaviour<IEnemyState>
{
    [SerializeField] float attackingDistance = 0.25f;
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float rotationSpeed = 2f;
    [SerializeField] Vector3 posOffset;
    [SerializeField] Transform rayPoint;
    [SerializeField] LayerMask hitLayer;

    private NavMeshPath path;
    bool hasPath = false;
    bool canMove = true;
    Vector3 target = Vector3.zero;
    RaycastHit hit;

    int posIndex;

    public override void Attached()
    {
        state.SetTransforms(state.EnemyTransform, transform);
        path = new NavMeshPath();
    }

    public override void SimulateOwner()
    {
        var groundOffset = Vector3.zero;
        if (Physics.Raycast(rayPoint.position, -rayPoint.up, out hit, 10f, hitLayer))
        {
            groundOffset = hit.point + posOffset;
        }

        if (path.corners.Length > 0)
        {
            if(posIndex != path.corners.Length && canMove)
            {
                var movement = Vector3.MoveTowards(transform.position, path.corners[posIndex], moveSpeed * BoltNetwork.FrameDeltaTime);
                transform.position = movement;
                var rotation = Quaternion.LookRotation(path.corners[posIndex] - transform.position);
                rotation.x = 0;
                rotation.z = 0;
                rotation.Normalize();
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * BoltNetwork.FrameDeltaTime);

                if (Vector3.Distance(transform.position, path.corners[posIndex]) < 0.25f)
                {
                    posIndex++;
                }
            }
        }

        for (int i = 0; i < path.corners.Length - 1; i++)
        Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
    }

    private void Update()
    {
        AttackTarget();
    }
    public void AttackTarget()
    {
        if (GetTargetDistance() <= attackingDistance)
        {
            canMove = false;
            //attacking code after
        }
    }

    public float GetTargetDistance()
    {
        return Vector3.Distance(transform.position, target);
    }

    public void CalculatePath(Vector3 target)
    {
        NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);
    }

    public void SetPath(Vector3 targetPos)
    {
        target = targetPos;

        CalculatePath(target);

        //agent.SetDestination(target);
        //agent.isStopped = true;
    }
}
