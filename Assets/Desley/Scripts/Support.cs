using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Support : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] HitDamageUI hitDamageUI;

    [Space, SerializeField] GameObject fireBall;
    [SerializeField] GameObject healBall;
    [SerializeField] GameObject stunBall;
    [SerializeField] Transform firePoint;
    GameObject ballToUse;

    [Space, SerializeField] float chargeForce;
    [SerializeField] float maxCharge, minCharge;
    bool isShooting;
    bool charging;
    float chargeTime;

    bool usingHeal, usingStun;
    bool canHitPlayer;

    public override void Attached()
    {
        base.Attached();
    }

    void Start() 
    { 
        ballToUse = fireBall; 
    }

    private void Update()
    {
        isShooting = Input.GetButton("Fire1");

        if (!usingStun)
        {
            if (isShooting && entity.IsOwner && !state.IsDead)
            {
                charging = true;
                chargeTime += Time.deltaTime;
                chargeTime = Mathf.Clamp(chargeTime, 0, maxCharge);
            }
            else if (charging)
            {
                if (chargeTime < minCharge) { return; }
                InstantiateBall(ballToUse, chargeTime, canHitPlayer, false);
                charging = false;
            }
        }
    }

    public void InstantiateBall(GameObject pickedBall, float finalChargeTime, bool hitPlayer, bool stunsEnemies)
    {
        var ball = BoltNetwork.Instantiate(pickedBall, firePoint.position, firePoint.rotation);
        ball.GetComponent<Balls>().SetHitDamageUI(hitDamageUI);
        ball.GetComponent<Balls>().SetPlayerHit(hitPlayer, stunsEnemies);
        ball.GetComponent<Balls>().playerEntity = GetComponentInParent<BoltEntity>();
        ball.GetComponent<Rigidbody>().AddRelativeForce(0, 0, chargeForce * finalChargeTime, ForceMode.Impulse);
        chargeTime = 0;
    }

    public void StunBall()
    {
        InstantiateBall(stunBall, maxCharge, false, true);
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
