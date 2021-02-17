using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateHealth : Bolt.EntityBehaviour<IGateState>
{
    [SerializeField] int gateStartingHealth;

    int currentHealth;
    bool isDestroyed = false;
    bool healthActive = false;
    bool isHost;
    PlayerController player;

    private void Awake()
    {
        currentHealth = gateStartingHealth;
    }

    public override void Attached()
    {      
        StartCoroutine(StartDelay());
    }

    void HealthCallback()
    {
        if (!state.IsDestroyed && healthActive)
        {
            currentHealth = state.Health;
            isDestroyed = state.IsDestroyed;

            if (state.Health <= 0 && entity.IsOwner)
            {
                state.IsDestroyed = true;
                if (state.IsDestroyed)
                {
                    ////restart game panel
                    //var request = RestartRequest.Create();
                    //request.SceneIndex = 1;
                    //request.Send();
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

    public void SetPlayer(PlayerController playerController)
    {
        player = playerController;
    }

    IEnumerator StartDelay()
    {
        yield return new WaitUntil(() => player);

        isHost = player.GetIfHost();

        yield return new WaitForSeconds(0.5f);

        if (isHost)
        {
            if (entity.IsOwner)
            {
                state.IsDestroyed = isDestroyed;
                state.Health = currentHealth;
                state.AddCallback("Health", HealthCallback);
            }
            healthActive = true;
        }
        else
        {
            BoltNetwork.Destroy(gameObject);
        }
    }

    public float GetCurrentHealthPercentage() { return 100f / gateStartingHealth * state.Health; }
}
