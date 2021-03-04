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
    bool gotShot;

    GameInfo gameInfo;
    string enemyTeamTag;

    [HideInInspector]
    public bool stopBleeding;
    bool alreadyBleeding;

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
        if (!gameInfo) { gameInfo = FindObjectOfType<GameInfo>(); }

        healthbar.UpdateHealthbar(GetCurrentHealthPercentage(), maxHealth, state.PlayerHealth);

        //if (Input.GetButtonDown("FireMode") && entity.IsOwner) { state.PlayerHealth -= 10; }

        if (maxHealth > defaultMaxHealth && Time.time >= timePerReduce)
        {
            maxHealth -= 1;
            timePerReduce = Time.time + 1 / 4f;
        }
        if (state.PlayerHealth > defaultMaxHealth && entity.IsOwner && !gotShot)
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
                    request.KillTrigger = false;
                    request.IsPlayer = true;
                    request.Send();
                }
            }
        }  
    }

    public void StartBleeding(float time, int bleedDamage, int bleedTimes) 
    {
        if (!alreadyBleeding)
        {
            alreadyBleeding = true;
            StartCoroutine(Bleeding(time, bleedDamage, bleedTimes));
        }
    }
    IEnumerator Bleeding(float time, int bleedDamage, int bleedTimes)
    {
        stopBleeding = false;
        for (int i = 0; i < bleedTimes; i++)
        {
            if (!stopBleeding)
            {
                if (entity.IsOwner)
                {
                    state.PlayerHealth -= bleedDamage;
                    animator.SetBool("isBleeding", true);
                }

                yield return new WaitForSeconds(time);
            }
            else
            {
                alreadyBleeding = false;
                animator.SetBool("isBleeding", false);
                StopCoroutine(nameof(Bleeding));
            }
        }

        alreadyBleeding = false;
        animator.SetBool("isBleeding", false);
        StopCoroutine(nameof(Bleeding));
    }

    public void RespawnPlayer()
    {
        //Create DestroyRequest, set entity to ent and then send
        var request = DestroyRequest.Create();
        request.Entity = GetComponentInParent<BoltEntity>();
        request.KillTrigger = true;
        request.IsPlayer = true;
        request.Send();
    }

    public void AddTeamKill()
    {
        var killRequest = TeamKillEvent.Create();
        killRequest.TeamKillString = enemyTeamTag;
        killRequest.Send();
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
                state.PlayerHealth = maxHealth;
                gotShot = true;
            }
            else if(maxHealth > defaultMaxHealth && damage < maxHealth - defaultMaxHealth)
            {
                maxHealth -= damage;
                damage = 0;
            }

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

    public void GetHealing(int healing, bool healStim)
    {
        if (entity.IsOwner)
        {
            animator.ResetTrigger("Healed");
            animator.SetTrigger("Healed");

            if(state.PlayerHealth == maxHealth && healStim)
            {
                maxHealth += healing;
                gotShot = false;
            }
            else if(state.PlayerHealth + healing > maxHealth)
            {
                healing = maxHealth - state.PlayerHealth;
            }

            state.PlayerHealth += healing;

            if (!healStim)
                stopBleeding = true;
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

    public void SetTags() 
    {
        if(tag == "Team1") { enemyTeamTag = "Team2"; }
        else if(tag == "Team2") { enemyTeamTag = "Team1"; }
    }
}
