﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : Bolt.EntityBehaviour<IEnemyState>
{
    [SerializeField] int maxHealth;
    [SerializeField] Image enemyHealthbar;
    [SerializeField] Transform healthbarCanvas;
    int currentHealth;

    public override void SimulateOwner()
    {
        enemyHealthbar.fillAmount = Mathf.Lerp(enemyHealthbar.fillAmount, GetCurrentHealthPercentage() / 100, 20 * BoltNetwork.FrameDeltaTime);
    }

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public override void Attached()
    {
        state.EnemyHealth = maxHealth;
        state.AddCallback("EnemyHealth", HealthCallback);
    }
    void HealthCallback()
    {
        currentHealth = state.EnemyHealth;

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

    public float GetCurrentHealthPercentage() { return 100f / maxHealth * currentHealth; }
}
