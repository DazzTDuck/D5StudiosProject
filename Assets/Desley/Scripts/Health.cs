using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] int currentHealth, maxHealth;
    [SerializeField] Healthbar healthbar;

    bool isDeadlocal;
    bool destroyRequestDone = false;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public override void Attached()
    {
        base.Attached();
        state.PlayerHealth = maxHealth;
        state.IsDead = isDeadlocal;
        state.AddCallback("PlayerHealth", HealthCallback);
    }

    private void Update()
    {
        healthbar.UpdateHealthbar(GetCurrentHealthPercentage(), maxHealth, state.PlayerHealth);
    }

    void HealthCallback()
    {
        currentHealth = state.PlayerHealth;
        isDeadlocal = state.IsDead;

        if (state.PlayerHealth <= 0)
        {
            state.IsDead = true;
            if (state.IsDead)
            {
                GetComponent<MeshRenderer>().enabled = false;
                GetComponent<Collider>().enabled = false;
                GetComponent<Rigidbody>().useGravity = false;

                if (!destroyRequestDone)
                {
                    //Create DestroyRequest, set entity to ent and then send
                    var request = DestroyRequest.Create();
                    request.Entity = GetComponentInParent<BoltEntity>();
                    request.IsPlayer = true;
                    request.Send();
                    destroyRequestDone = true;
                }

            }
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
            state.PlayerHealth = maxHealth;
            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<Collider>().enabled = true;
            GetComponent<Rigidbody>().useGravity = true;
            destroyRequestDone = false;
        }
    }

    public int GetCurrentHealth() { return state.PlayerHealth; }
    public float GetCurrentHealthPercentage() { return 100f / maxHealth * state.PlayerHealth; }
}
