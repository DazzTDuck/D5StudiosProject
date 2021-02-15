using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Support : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] HitDamageUI hitDamageUI;

    [Space, SerializeField] GameObject fireBall;
    [SerializeField] GameObject healBall;
    [SerializeField] Transform firePoint;
    GameObject ballToUse;

    [Space, SerializeField] float chargeForce;
    [SerializeField] float maxCharge, minCharge;
    bool isShooting;
    bool charging;
    float chargeTime;

    public bool usingHeal, usingStun;
    bool canHitPlayer;

    public override void Attached()
    {
        base.Attached();
    }

    void Start() { ballToUse = fireBall; }

    private void Update()
    {
        isShooting = Input.GetButton("Fire1");

        if (isShooting && entity.IsOwner && !state.IsDead && !usingStun)
        {
            charging = true;
            chargeTime += Time.deltaTime;
            chargeTime = Mathf.Clamp(chargeTime, 0, maxCharge);
        }
        else if (charging)
        {
            if(chargeTime < minCharge) { return; }
            InstantiateBall();
            charging = false;
        }
    }

    public void InstantiateBall()
    {
        var ball = BoltNetwork.Instantiate(ballToUse, firePoint.position, firePoint.rotation);
        ball.GetComponent<Balls>().SetHitDamageUI(hitDamageUI);
        ball.GetComponent<Balls>().SetPlayerHit(canHitPlayer, usingHeal);
        ball.GetComponent<Balls>().playerEntity = GetComponentInParent<BoltEntity>();
        ball.GetComponent<Rigidbody>().AddRelativeForce(0, 0, chargeForce * chargeTime, ForceMode.Impulse);
        chargeTime = 0;
    }

    public void ChangeBalls()
    {
        if (!usingHeal)
        {
            //make ball heal
            ballToUse = healBall;
            canHitPlayer = true;
            usingHeal = true;
        }
        else
        {
            //make ball damage
            ballToUse = fireBall;
            canHitPlayer = false;
            usingHeal = false;
        }
    }
}
