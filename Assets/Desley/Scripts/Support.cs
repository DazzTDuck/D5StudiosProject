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
    [SerializeField] float powerUpDuration;
    float multiplier = 1;
    bool isShooting;
    bool charging;
    float chargeTime;

    [Space, SerializeField] float disableShootingTime = .5f;
    bool usingHeal;
    bool usingAbility;

    string teamTag;
    string enemyTeamTag;

    void Start() 
    { 
        ballToUse = fireBall; 
    }

    private void Update()
    {
        isShooting = Input.GetButton("Fire1") && !state.IsStunned;

        if (!usingAbility)
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
        Balls balls = ball.GetComponent<Balls>();

        //Setup ball
        balls.SetHitDamageUI(hitDamageUI);
        balls.SetTags(teamTag, enemyTeamTag);
        balls.SetPlayerHit(healBall, stunsEnemies);
        balls.DetermineDamage(state.IsPoweredUp);
        balls.playerEntity = GetComponentInParent<BoltEntity>();
        balls.GetComponent<Rigidbody>().AddRelativeForce(0, 0, chargeForce * finalChargeTime, ForceMode.Impulse);

        chargeTime = 0;
    }

    public void StunBall()
    {
        StartCoroutine(DisableShooting(disableShootingTime));

        InstantiateBall(stunBall, maxCharge, false, true);
    }

    public void ChangeBalls()
    {
        StartCoroutine(DisableShooting(disableShootingTime));

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

    public void PowerUp()
    {
        StartCoroutine(DisableShooting(disableShootingTime));

        var request = TeamBoostRequest.Create();
        request.TeamTagString = teamTag;
        request.Duration = powerUpDuration;
        request.Send();
    }

    IEnumerator DisableShooting(float time)
    {
        usingAbility = true;

        yield return new WaitForSeconds(time);

        usingAbility = false;

        StopCoroutine(nameof(DisableShooting));
    }

    public void SetTags()
    {
        teamTag = GetComponentInParent<Health>().tag;
        if (teamTag == "Team1") { enemyTeamTag = "Team2"; }
        else if (teamTag == "Team2") { enemyTeamTag = "Team1"; }
    }
}
