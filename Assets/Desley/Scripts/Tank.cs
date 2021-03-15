using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [Space, SerializeField] PauseMenuHandler pauseMenuHandler;
    [SerializeField] Camera cam;
    [SerializeField] HitDamageUI hitDamageUI;
    [SerializeField] Transform shankPoint;
    [SerializeField] int damage, powerUp;
    [SerializeField] float shankRate, range, damageTime;
    float nextTimeToShank;

    [Space, SerializeField] int bleedDamage;
    [SerializeField] int bleedTimes;
    [SerializeField] float timeInBetween;
    [SerializeField] float canBleedTime;

    string teamTag;
    string enemyTeamTag;
    
    Collider[] hitObjects;
    List<GameObject> hitEnemies = new List<GameObject>();

    bool isShooting;

    [Space, SerializeField] float animationResetAddTime;
    float animationResetTime;
    int attackAnimationIndex;

    bool shieldClosed;

    [SerializeField] float canAttackAgainTime;
    bool canAttack = true;

    public void Update()
    {
        isShooting = Input.GetButton("Fire1") && canAttack && !pauseMenuHandler.GetIfPaused() && !state.IsStunned;

        if (isShooting && Time.time >= nextTimeToShank && !state.IsDead && entity.IsOwner)
        {
            StartCoroutine(WaitForAnimation(damageTime));
            animationResetTime = animationResetAddTime;

            nextTimeToShank = Time.time + 1f / shankRate;

            //switch between left and right
            if(attackAnimationIndex == 0) { attackAnimationIndex = 1; }
            else { attackAnimationIndex = 0; }
        }

        if (animationResetTime != Mathf.Infinity)
            ShankAnimationHandler();
    }

    void ShankAnimationHandler()
    {
        animationResetTime -= Time.deltaTime;
        if(animationResetTime <= 0)
        {
            attackAnimationIndex = 0;
            animationResetTime = Mathf.Infinity;
        }
    }

    public void ChangeShieldStance()
    {
        shieldClosed = !shieldClosed;
        if (shieldClosed) 
        { 
            canAttack = false;
            GetComponentInParent<PlayerController>().shieldClosed = true;
        }
        else { StartCoroutine(CanAttackAgain(canAttackAgainTime)); }

        string animationString;
        if (shieldClosed)
            animationString = "ShieldUp";
        else
            animationString = "ShieldDown";

        var request = TankAnimationsEvent.Create();
        request.TankEntity = GetComponentInParent<BoltEntity>();
        request.AnimationTriggerString = animationString;
        request.Send();
    }

    IEnumerator CanAttackAgain(float time)
    {
        yield return new WaitForSeconds(time);

        canAttack = true;
        GetComponentInParent<PlayerController>().shieldClosed = false;
    }

    public override void Attached()
    {
        state.AddCallback("IsPoweredUp", DamageCallback);

        if (entity.IsOwner)
        {
            int index = 9;
            gameObject.layer = index;
            var transforms = GetComponentsInChildren<Transform>();

            foreach (var gameObject in transforms)
            {
                gameObject.gameObject.layer = index;
            }
        }
    }

    void DamageCallback() { damage = state.IsPoweredUp ? damage *= powerUp : damage /= powerUp; }

    public void Shank()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range))
        {
            if (hit.collider.gameObject.layer == 15)
                return;

            BoltEntity boltEntity = hit.collider.GetComponentInParent<BoltEntity>();

            if (boltEntity)
            {
                Health health = boltEntity.GetComponentInChildren<Health>();

                if (health && health.CompareTag(enemyTeamTag))
                {
                    SendDamage(damage, boltEntity);
                }
            }
        }
    }

    void SendDamage(int damage, BoltEntity entityShot)
    {
        var request = DamageRequest.Create();
        request.EntityShot = entityShot;
        request.Damage = damage;
        request.EntityShooter = entity;
        request.Send();

        SendBleedEffect(entityShot);

        hitDamageUI.SendDamage(damage, true);
    }

    void SendBleedEffect(BoltEntity entityShot)
    {
        var request = BleedEffectEvent.Create();
        request.EntityShot = entityShot;
        request.Damage = bleedDamage;
        request.BleedTimes = bleedTimes;
        request.TimeInBetween = timeInBetween;
        request.Send();
    }

    public void TeamCanBleed()
    {
        var request = GiveTeamBleed.Create();
        request.TeamTagString = teamTag;
        request.Duration = canBleedTime;
        request.Send();
    }

    public IEnumerator WaitForAnimation(float time)
    {
        SendAnimation();

        yield return new WaitForSeconds(time);

        Shank();

        StopCoroutine(nameof(WaitForAnimation));
    }

    void SendAnimation()
    {
        string animationString;
        if (attackAnimationIndex == 0)
            animationString = "AttackLeft";
        else
            animationString = "AttackRight";

        var request = TankAnimationsEvent.Create();
        request.TankEntity = GetComponentInParent<BoltEntity>();
        request.AnimationTriggerString = animationString;
        request.Send();
    }

    public void SetTags()
    {
        teamTag = GetComponentInParent<Health>().tag;
        if (teamTag == "Team1") { enemyTeamTag = "Team2"; }
        else if (teamTag == "Team2") { enemyTeamTag = "Team1"; }
    }
}
