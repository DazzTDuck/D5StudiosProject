using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] int currentHealth, maxHealth;

    public override void Attached()
    {
        base.Attached();
    }

    public override void SimulateOwner()
    {
        if(currentHealth <= 0)
        {
            Debug.LogWarning("sack");
            BoltNetwork.Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
    }
}
