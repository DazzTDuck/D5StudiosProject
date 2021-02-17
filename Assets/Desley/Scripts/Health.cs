using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] Healthbar healthbar;
    [SerializeField] Animator animator;
    [SerializeField] int maxHealth;
    float timePerReduce;
    int defaultMaxHealth;

    bool isDeadlocal;

    private void Awake()
    {
        defaultMaxHealth = maxHealth;
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

        if (Input.GetButtonDown("FireMode")) { state.PlayerHealth -= 10; }

        if(maxHealth > defaultMaxHealth && Time.time >= timePerReduce)
        {
            maxHealth -= 1;
            timePerReduce = Time.time + 1 / 5f;
        }
        if(state.PlayerHealth > defaultMaxHealth)
        {
            state.PlayerHealth = maxHealth;
        }
    }

    void HealthCallback()
    {
        if (!state.IsDead)
        {
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
            if(maxHealth > defaultMaxHealth && damage > maxHealth - defaultMaxHealth)
            {
                int usedDamage = maxHealth - defaultMaxHealth;
                maxHealth -= usedDamage;
                damage -= usedDamage;
            }
            else if (state.PlayerHealth - damage <= 0)
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
            animator.ResetTrigger("Healed");
            animator.SetTrigger("Healed");

            if(state.PlayerHealth == maxHealth)
            {
                maxHealth += healing;
            }
            else if(state.PlayerHealth + healing > maxHealth)
            {
                healing = maxHealth - state.PlayerHealth;
            }

            state.PlayerHealth += healing;
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
