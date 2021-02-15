using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Support : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] HitDamageUI hitDamageUI;

    [Space, SerializeField] GameObject fireBall;
    [SerializeField] Transform firePoint;

    [Space, SerializeField] float chargeForce;
    [SerializeField] float maxCharge, minCharge;
    bool isShooting;
    bool charging;
    float chargeTime;

    public bool usingHeal, usingStun;

    public override void Attached()
    {
        base.Attached();
    }

    private void Update()
    {
        isShooting = Input.GetButton("Fire1");

        if (isShooting && entity.IsOwner && !state.IsDead && !usingHeal && !usingStun)
        {
            charging = true;
            chargeTime += Time.deltaTime;
            chargeTime = Mathf.Clamp(chargeTime, 0, maxCharge);
        }
        else if (charging)
        {
            if(chargeTime < minCharge) { return; }
            InstantiateFireBall();
            charging = false;
        }
    }

    public void InstantiateFireBall()
    {
        var fireball = BoltNetwork.Instantiate(fireBall, firePoint.position, firePoint.rotation);
        fireball.GetComponent<FireBall>().SetHitDamageUI(hitDamageUI);
        fireball.GetComponent<FireBall>().playerEntity = GetComponentInParent<BoltEntity>();
        fireball.GetComponent<Rigidbody>().AddRelativeForce(0, 0, chargeForce * chargeTime, ForceMode.Impulse);
        chargeTime = 0;
    }
}
