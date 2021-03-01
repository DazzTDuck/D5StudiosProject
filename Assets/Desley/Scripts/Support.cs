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
    float multiplier = 1;
    bool isShooting;
    bool charging;
    float chargeTime;

    bool usingHeal, usingStun;
    public bool isStunned;

    string teamTag;
    string enemyTeamTag;

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
        isShooting = Input.GetButton("Fire1") && !isStunned;

        if (!usingStun)
        {
            if (isShooting && entity.IsOwner && !state.IsDead)
            {
                charging = true;
                chargeTime += Time.deltaTime * multiplier;
                chargeTime = Mathf.Clamp(chargeTime, 0, maxCharge);
            }
            else if (charging)
            {
                if (chargeTime < minCharge) { return; }
                InstantiateBall(ballToUse, chargeTime, usingHeal, false);
                charging = false;
            }
        }
    }

    public void InstantiateBall(GameObject pickedBall, float finalChargeTime, bool healBall, bool stunsEnemies)
    {
        var ball = BoltNetwork.Instantiate(pickedBall, firePoint.position, firePoint.rotation);
        ball.GetComponent<Balls>().SetHitDamageUI(hitDamageUI);
        ball.GetComponent<Balls>().SetTags(teamTag, enemyTeamTag);
        ball.GetComponent<Balls>().SetPlayerHit(healBall, stunsEnemies);
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
            usingHeal = true;
        }
        else
        {
            //make ball damage
            ballToUse = fireBall;
            usingHeal = false;
        }
    }

    public void SendTeamBoost()
    {

    }

    public void SetTags()
    {
        teamTag = GetComponentInParent<Health>().tag;
        if (teamTag == "Team1") { enemyTeamTag = "Team2"; }
        else if (teamTag == "Team2") { enemyTeamTag = "Team1"; }
    }
}
