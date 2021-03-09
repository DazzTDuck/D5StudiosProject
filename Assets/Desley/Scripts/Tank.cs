using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [Space, SerializeField] PauseMenuHandler pauseMenuHandler;
    [SerializeField] Camera cam;
    [SerializeField] Animator animator;
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

    public void Update()
    {
        isShooting = Input.GetButton("Fire1") && !pauseMenuHandler.GetIfPaused() && !state.IsStunned;

        if (isShooting && entity.IsOwner && Time.time >= nextTimeToShank && !state.IsDead)
        {
            animator.ResetTrigger("Bonk");
            animator.SetTrigger("Bonk");
            StartCoroutine(WaitForAnimation(damageTime));
            nextTimeToShank = Time.time + 1f / shankRate;
        }
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
        yield return new WaitForSeconds(time);

        Shank();

        StopCoroutine(nameof(WaitForAnimation));
    }

    public void SetTags()
    {
        teamTag = GetComponentInParent<Health>().tag;
        if (teamTag == "Team1") { enemyTeamTag = "Team2"; }
        else if (teamTag == "Team2") { enemyTeamTag = "Team1"; }
    }
}
