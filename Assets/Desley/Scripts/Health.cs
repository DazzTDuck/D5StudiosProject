using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] int currentHealth, maxHealth;
    [SerializeField] Healthbar healthbar;

    bool isDeadlocal;

    public override void Attached()
    {
        base.Attached();
        currentHealth = maxHealth;
        state.PlayerHealth = currentHealth;
        state.IsDead = isDeadlocal;
        state.AddCallback("PlayerHealth", HealthCallback);
    }

    private void Update()
    {
        healthbar.UpdateHealthbar(GetCurrentHealthPercentage(), maxHealth, currentHealth);
    }

    void HealthCallback()
    {
        currentHealth = state.PlayerHealth;
        isDeadlocal = state.IsDead;

        if (currentHealth <= 0)
        {
            state.IsDead = true;
            if (state.IsDead)
            {
                GetComponent<MeshRenderer>().enabled = false;
                GetComponent<Collider>().enabled = false;
            }
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
        state.IsDead = false;
        if (!state.IsDead)
        {
            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<Collider>().enabled = true;
        }
        currentHealth = maxHealth;
        state.PlayerHealth = currentHealth;
    }

    public int GetCurrentHealth() { return currentHealth; }
    public float GetCurrentHealthPercentage() { return 100f / maxHealth * currentHealth; }
}
