using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] Camera cam;
    [SerializeField] GameObject flash;
    [SerializeField] Transform muzzle;
    [SerializeField] int damage, range;
    [SerializeField] float flashTimer = Mathf.Infinity;

    Health health;
    EnemyHealth enemyHealth;

    bool isShooting;

    private void Update()
    {
        isShooting = Input.GetButtonDown("Fire1");

        if (isShooting && entity.IsOwner)
        {
            state.WeaponTrigger();
            flash.SetActive(true);
            flashTimer = .1f;
        }
    }

    public override void Attached()
    {
        base.Attached();
        state.OnWeaponTrigger = ShootRaycast;
    }

    public override void SimulateOwner()
    {
        base.SimulateOwner();

        MuzzleFlash();
    }

    public void ShootRaycast()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            health = hit.collider.gameObject.GetComponent<Health>();
            if (health)
            {
                //Create DamageRequest, set entity to ent and Damage to damage, then send
                var request = DamageRequest.Create();
                request.Entity = health.GetComponentInParent<BoltEntity>();
                request.Damage = damage;
                request.Send();
                health = null;
            }

            enemyHealth = hit.collider.gameObject.GetComponent<EnemyHealth>();
            if (enemyHealth)
            {
                enemyHealth.TakeDamage(damage);
                enemyHealth = null;
            }
        }
    }

    public void MuzzleFlash()
    {
        flashTimer -= Time.deltaTime;

        if (flashTimer <= 0)
        {
            flash.SetActive(false);
            flashTimer = Mathf.Infinity;
        }

    }
}
