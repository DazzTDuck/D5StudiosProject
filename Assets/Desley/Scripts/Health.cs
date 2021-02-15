using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] int currentHealth, maxHealth;
    [SerializeField] Healthbar healthbar;

    bool isDeadlocal;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public override void Attached()
    {
        if (entity.IsOwner)
        {
            state.PlayerHealth = maxHealth;
            state.IsDead = isDeadlocal;
            state.AddCallback("PlayerHealth", HealthCallback);
        }
    }

    private void Update()
    {
        healthbar.UpdateHealthbar(GetCurrentHealthPercentage(), maxHealth, state.PlayerHealth);
    }

    void HealthCallback()
    {
        if (!state.IsDead)
        {
            currentHealth = state.PlayerHealth;
            isDeadlocal = state.IsDead;

            if (state.PlayerHealth <= 0)
            {
                state.IsDead = true;
                if (state.IsDead)
                {
                    Debug.LogWarning("dead");

                    GetComponent<MeshRenderer>().enabled = false;
                    GetComponent<Collider>().enabled = false;
                    GetComponent<Rigidbody>().useGravity = false;

                    //Create DestroyRequest, set entity to ent and then send
                    var request = DestroyRequest.Create();
                    request.Entity = GetComponentInParent<BoltEntity>();
                    request.IsPlayer = true;
                    request.Send();
                }
            }
        }  
    }

    public void TakeDamage(int damage)
    {
        if (entity.IsOwner)
        {
            if (state.PlayerHealth - damage <= 0)
            {
                state.PlayerHealth = 0;
            }
            else
            {
                state.PlayerHealth -= damage;
            }
        }
    }

    public void GetHealing(int healing)
    {
        if (entity.IsOwner)
        {
            if(state.PlayerHealth + healing > maxHealth)
            {
                healing = maxHealth - state.PlayerHealth;
            }
            else
            {
                state.PlayerHealth += healing;
            }
        }
    }

    public void ResetHealth()
    {
        if (entity.IsOwner)
            state.IsDead = false;

        if (!state.IsDead)
        {
            state.PlayerHealth = maxHealth;
            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<Collider>().enabled = true;
            GetComponent<Rigidbody>().useGravity = true;
        }
    }

    public int GetCurrentHealth() { return state.PlayerHealth; }
    public float GetCurrentHealthPercentage() { return 100f / maxHealth * state.PlayerHealth; }
}
