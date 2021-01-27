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

    public override void Attached()
    {
        base.Attached();
        state.OnWeaponTrigger = ShootRaycast;
    }

    public override void SimulateOwner()
    {
        base.SimulateOwner();
        if (Input.GetButtonDown("Fire1") && entity.IsOwner)
        {
            state.WeaponTrigger();
            flash.SetActive(true);
            flashTimer = .1f;
        }

        flashTimer -= Time.deltaTime;

        if(flashTimer <= 0)
        {
            flash.SetActive(false);
            flashTimer = Mathf.Infinity;
        }
    }

    public void ShootRaycast()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            Health health = hit.collider.gameObject.GetComponent<Health>();
            if(health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }
}
