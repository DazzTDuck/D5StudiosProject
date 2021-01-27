using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : Bolt.EntityBehaviour<IEnemyState>
{
    [SerializeField] int maxHealth;
    [SerializeField] int currentHealth;

    public override void Attached()
    {
        currentHealth = maxHealth;
        state.EnemyHealth = currentHealth;
        state.AddCallback("EnemyHealth", HealthCallback);
    }
    void HealthCallback()
    {
        currentHealth = state.EnemyHealth;

        if (currentHealth <= 0)
        {
            //Create DestroyRequest, set entity to ent and then send
            var request = DestroyRequest.Create();
            request.Entity = GetComponent<BoltEntity>();
            request.Send();
        }
    }

    public void TakeDamage(int damage)
    {
        state.EnemyHealth -= damage;
    }
}
