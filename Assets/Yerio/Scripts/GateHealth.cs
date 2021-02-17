using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateHealth : Bolt.EntityBehaviour<IGateState>
{
    [SerializeField] int gateStartingHealth;

    int currentHealth;
    bool isDestroyed = false;

    private void Awake()
    {
        currentHealth = gateStartingHealth;
    }

    public override void Attached()
    {
        if (entity.IsOwner)
        {
            state.IsDestroyed = isDestroyed;
            state.Health = currentHealth;
            state.AddCallback("Health", HealthCallback);
        }
    }

    void HealthCallback()
    {
        if (!state.IsDestroyed)
        {
            currentHealth = state.Health;
            isDestroyed = state.IsDestroyed;

            if (state.Health <= 0 && entity.IsOwner)
            {
                state.IsDestroyed = true;
                if (state.IsDestroyed)
                {
                    //restart game panel
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (entity.IsOwner)
        {
            if (state.Health - damage <= 0)
            {
                state.Health = 0;
            }
            else
            {
                state.Health -= damage;
            }
        }
    }

    public float GetCurrentHealthPercentage() { return 100f / gateStartingHealth * state.Health; }
}
