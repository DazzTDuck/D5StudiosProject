﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : Bolt.EntityBehaviour<IEnemyState>
{
    [SerializeField] int maxHealth;
    [SerializeField] int currentHealth;
    [SerializeField] Image enemyHealthbar;
    [SerializeField] Transform healthbarCanvas;

    private void Update()
    {
        enemyHealthbar.fillAmount = Mathf.Lerp(enemyHealthbar.fillAmount, GetCurrentHealthPercentage() / 100, 20 * BoltNetwork.FrameDeltaTime);
    }

    public override void Attached()
    {
        state.EnemyHealth = currentHealth;
        state.AddCallback("EnemyHealth", HealthCallback);
    }
    void HealthCallback()
    {
        currentHealth = state.EnemyHealth;
        //enemyHealthbar.fillAmount = GetCurrentHealthPercentage() / 100;

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

    public void CanvasLookAt(Transform transform)
    {
        healthbarCanvas.LookAt(transform);
    }

    public float GetCurrentHealthPercentage() { return 100f / maxHealth * state.EnemyHealth; }
}
