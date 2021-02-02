using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : Bolt.EntityBehaviour<IEnemyState>
{
    [SerializeField] int maxHealth;
    [SerializeField] int currentHealth;
    [SerializeField] Image enemyHealthbar;
    [SerializeField] Transform healthbarCanvas;

    bool isDeadLocal;
    private void Update()
    {
        enemyHealthbar.fillAmount = Mathf.Lerp(enemyHealthbar.fillAmount, GetCurrentHealthPercentage() / 100, 20 * BoltNetwork.FrameDeltaTime);
    }

    public override void Attached()
    {
        if (entity.IsOwner)
        {
            state.EnemyHealth = currentHealth;
            state.IsDead = isDeadLocal;
            state.AddCallback("EnemyHealth", HealthCallback);
        }
    }
    void HealthCallback()
    {
        if (!state.IsDead)
        {
            currentHealth = state.EnemyHealth;
            isDeadLocal = state.IsDead;
            //enemyHealthbar.fillAmount = GetCurrentHealthPercentage() / 100;

            if (currentHealth <= 0)
            {
                state.IsDead = true;
                //Create DestroyRequest, set entity to ent and then send
                var request = DestroyRequest.Create();
                request.Entity = GetComponent<BoltEntity>();
                request.IsEnemy = true;
                request.Send();
            }
        }

    }

    public void TakeDamage(int damage)
    {
        if (entity.IsOwner)
        {
            state.EnemyHealth -= damage;
        }
    }

    public void CanvasLookAt(Transform transform)
    {
        healthbarCanvas.LookAt(transform);
    }

    public float GetCurrentHealthPercentage() { return 100f / maxHealth * state.EnemyHealth; }
}
