using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] int currentHealth, maxHealth;
    [SerializeField] Healthbar healthbar;

    public override void Attached()
    {
        base.Attached();
        currentHealth = maxHealth;
        state.PlayerHealth = currentHealth;
        state.AddCallback("PlayerHealth", HealthCallback);
        healthbar.UpdateHealthbar(GetCurrentHealthPercentage(), maxHealth, currentHealth);
    }

    void HealthCallback()
    {
        currentHealth = state.PlayerHealth;

        healthbar.UpdateHealthbar(GetCurrentHealthPercentage(), maxHealth, currentHealth);

        if (currentHealth <= 0)
        {
            //Create DestroyRequest, set entity to ent and then send
            var request = DestroyRequest.Create();
            request.Entity = GetComponentInParent<BoltEntity>();
            request.IsPlayer = true;
            request.Send();
        }
    }

    public void TakeDamage(int damage)
    {
        state.PlayerHealth -= damage;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        state.PlayerHealth = currentHealth;
        healthbar.UpdateHealthbar(GetCurrentHealthPercentage(), maxHealth, currentHealth);
    }

    public int GetCurrentHealth() { return currentHealth; }
    public float GetCurrentHealthPercentage() { return 100f / maxHealth * currentHealth; }
}
