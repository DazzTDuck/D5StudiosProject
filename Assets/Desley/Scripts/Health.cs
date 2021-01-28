using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] int currentHealth, maxHealth;

    public override void Attached()
    {
        base.Attached();
        currentHealth = maxHealth;
        state.PlayerHealth = currentHealth;
        state.AddCallback("PlayerHealth", HealthCallback);
    }

    void HealthCallback()
    {
        currentHealth = state.PlayerHealth;

        if (currentHealth <= 0)
        {
            //Create DestroyRequest, set entity to ent and then send
            var request = DestroyRequest.Create();
            request.Entity = GetComponentInParent<BoltEntity>();
            request.Send();
        }
    }

    public void TakeDamage(int damage)
    {
        state.PlayerHealth -= damage;
    }
}
