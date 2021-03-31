using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Support : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] HitDamageUI hitDamageUI;
    [SerializeField] Image chargeBar;
    [SerializeField] GameObject chargeBarBacking;
    [SerializeField] Animator animator;

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

    string teamTag;
    string enemyTeamTag;

    [Space, SerializeField] float animationTimer = .5f;

    void Start() 
    { 
        ballToUse = fireBall; 
    }

    private void Update()
    {
        isShooting = Input.GetButton("Fire1") && !state.IsStunned;

        if (!state.IsUsingAbility)
        {
            if (isShooting && entity.IsOwner && !state.IsDead)
            {
                charging = true;
                chargeTime += Time.deltaTime * multiplier;
                chargeTime = Mathf.Clamp(chargeTime, 0, maxCharge);

                chargeBar.gameObject.SetActive(true);
                chargeBarBacking.SetActive(true);
                chargeBar.fillAmount = GetPercentage(maxCharge, chargeTime) / 100f;
            }
            else if (charging)
            {
                if (chargeTime < minCharge) { return; }
                InstantiateBall(ballToUse, chargeTime, usingHeal, false);
                charging = false;
                chargeBar.fillAmount = 0;
                chargeBar.gameObject.SetActive(false);
                chargeBarBacking.SetActive(false);
            }
        }
    }

    public float GetPercentage(float max, float current) { return 100f / max * current; }

    public void InstantiateBall(GameObject pickedBall, float finalChargeTime, bool healBall, bool stunsEnemies)
    {
        animator.SetTrigger("Fire");

        var ball = BoltNetwork.Instantiate(pickedBall, firePoint.position, firePoint.rotation);
        Balls balls = ball.GetComponent<Balls>();

        //Setup ball
        balls.SetHitDamageUI(hitDamageUI);
        balls.SetTags(teamTag, enemyTeamTag);
        balls.SetPlayerHit(healBall, stunsEnemies);
        balls.DetermineDamage(state.IsPoweredUp);
        balls.playerEntity = GetComponentInParent<BoltEntity>();
        balls.canBleedEnemies = state.CanBleedEnemies;
        balls.GetComponent<Rigidbody>().AddRelativeForce(0, 0, chargeForce * finalChargeTime, ForceMode.Impulse);

        chargeTime = 0;
    }

    public void StunBall()
    {
        StartCoroutine(DisableShooting(disableShootingTime));

        StartCoroutine(WaitForAnimation(animationTimer));
    }

    IEnumerator WaitForAnimation(float time)
    {
        //play animation
        animator.SetTrigger("Fire");

        StartCoroutine(DisableShooting(disableShootingTime));

        yield return new WaitForSeconds(time);

        if(state.StopAbilities)
            yield break;

        InstantiateBall(stunBall, maxCharge, false, true);

        StopCoroutine(nameof(WaitForAnimation));
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
        state.IsUsingAbility = true;

        yield return new WaitForSeconds(time);

        state.IsUsingAbility = false;

        StopCoroutine(nameof(DisableShooting));
    }

    public void SetTags()
    {
        teamTag = GetComponentInParent<Health>().tag;
        if (teamTag == "Team1") { enemyTeamTag = "Team2"; }
        else if (teamTag == "Team2") { enemyTeamTag = "Team1"; }
    }
}
