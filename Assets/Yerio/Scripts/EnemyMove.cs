using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMove : Bolt.EntityBehaviour<IEnemyState>
{
    [SerializeField] float attackingDistance = 2f;
    [SerializeField] float moveSpeed = 20f;
    [SerializeField] int attackDamage = 3;
    [SerializeField] float timeBetweenAttack = 2;
    [SerializeField] Timer stunTimer;
    [SerializeField] Timer attackTimer;
    
    NavMeshAgent agent;
    Vector3 target = Vector3.zero;

    bool canAttack = false;
    bool isStunned;
    GateHealth gateHealth;

    public override void Attached()
    {
        state.SetTransforms(state.EnemyTransform, transform);     
    }

    public override void SimulateOwner()
    {
        if(!gateHealth) { gateHealth = FindObjectOfType<GateHealth>(); }

        AttackTarget();
    }
    public void AttackTarget()
    {
        if (gateHealth)
        {
            if (GetTargetDistance() <= attackingDistance && entity.IsOwner)
            {
                agent.isStopped = true;

                if (attackTimer.IsTimerComplete() && gateHealth.state.Health > 0 && !isStunned)
                {
                    attackTimer.SetTimer(timeBetweenAttack, () => canAttack = true, () => canAttack = false);
                }

                if (canAttack && !isStunned)
                {
                    gateHealth.TakeDamage(attackDamage);
                    canAttack = false;
                }
            }
        } 
    }

    public void StunEnemy(float time)
    {
        if (stunTimer.IsTimerComplete())
        {
            stunTimer.SetTimer(time, () => { agent.isStopped = false; isStunned = false; }, 
                () => { agent.isStopped = true; isStunned = true; });
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
        }
    }
}
