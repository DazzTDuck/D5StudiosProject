﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Scout : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [Space, SerializeField] PauseMenuHandler pauseMenuHandler;

    [SerializeField] Camera cam, weaponCam;
    [SerializeField] Animator animator;
    [SerializeField] HitDamageUI hitDamageUI;

    [Space, SerializeField] GameObject BulletCountCanvas;
    [SerializeField] TMP_Text bulletCountText;
    [SerializeField] GameObject reloadingText;

    [Space, SerializeField] GameObject flash;
    [SerializeField] GameObject bulletHit;
    [SerializeField] Transform muzzle;

    [Space, SerializeField] int damage, hsMultiplier, range;
    [SerializeField] float fireRate, weaponPunchX;

    [Space, SerializeField] int currentBulletCount;
    [SerializeField] int maxBulletCount;
    [SerializeField] float reloadTime;
    [SerializeField] bool reloading;

    [Space, SerializeField] int shotgunPellets;
    [SerializeField] float randomSpread;
    [SerializeField] int spreadDivider;
    Vector3 spread;

    [Space, SerializeField] float dropoffRange;
    [SerializeField] int dropOffDivider;
    int damageDivider = 1;

    float nextTimeToShoot;
    bool isShooting;
    bool nextShot;
    bool shootingDisabled;

    string teamTag;
    string enemyTeamTag;

    [Space, SerializeField] GameObject knife;
    [SerializeField] Transform[] throwPoints;
    [SerializeField] float force;


    private void Update()
    {
        CheckFireModeInput();
        CheckReloadInput();

        if(!shootingDisabled && entity.IsOwner && currentBulletCount > 0 && !reloading && Time.time >= nextTimeToShoot && !state.IsDead)
        {
            if (isShooting || nextShot)
            {
                state.Animator.ResetTrigger("Shoot");
                state.Animator.SetTrigger("Shoot");
                ShootRaycast();
                InstantiateEffect();
                nextTimeToShoot = Time.time + 1f / fireRate;
                weaponCam.GetComponent<PlayerCamera>().AddRecoil(weaponPunchX, 0);
                cam.GetComponent<PlayerCamera>().AddRecoil(weaponPunchX, 0);
                nextShot = false;
                currentBulletCount--;
            }
        }
    }

    void CheckFireModeInput()
    {
        //check input of mouse
        isShooting = Input.GetButtonDown("Fire1") && nextTimeToShoot < Time.time && !pauseMenuHandler.GetIfPaused();

        //check for input in between chambering rounds
        if (nextTimeToShoot > Time.time && Input.GetButtonDown("Fire1") && !isShooting)
            nextShot = true;
        else if (nextTimeToShoot > Time.time && Input.GetButtonUp("Fire1"))
            nextShot = false;
    }

    void CheckReloadInput()
    {
        if (Input.GetButtonDown("Reload") && !pauseMenuHandler.GetIfPaused() && !reloading && nextTimeToShoot < Time.time && currentBulletCount != maxBulletCount || currentBulletCount == 0 && !reloading && nextTimeToShoot < Time.time)
        {
            StartCoroutine(Reload(reloadTime));
            reloading = true;
        }
        if (!reloading)
            bulletCountText.text = currentBulletCount + "|" + maxBulletCount;
    }

    //public bool GetIfShooting() { return isShooting && Time.time >= nextTimeToShoot; }

    public override void Attached()
    {
        base.Attached();
        state.SetAnimator(animator);

        if (entity.IsOwner)
        {
            int index = 9; //Weapon layer
            gameObject.layer = index;
            BulletCountCanvas.layer = 10;
            reloadingText.gameObject.layer = 10; //HUD layer
            bulletCountText.gameObject.layer = 10;
            var transforms = GetComponentsInChildren<Transform>();

            foreach (var gameObject in transforms)
            {
                gameObject.gameObject.layer = index;
            }
        }
    }

    public void ShootRaycast()
    { 
        int totalDamage = 0;
        int amountShot = 0;

        for (var i = 0; i < shotgunPellets; i++)
        {
            amountShot++;

            var randomX = Random.Range(-randomSpread, randomSpread);
            var randomY = Random.Range(-randomSpread, randomSpread);
            spread = new Vector3(randomX, randomY, 0);

            Ray ray = weaponCam.ScreenPointToRay(Input.mousePosition + spread);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, range))
            {
                var hitEffect = BoltNetwork.Instantiate(bulletHit, hit.point, Quaternion.identity);
                StartCoroutine(DestroyEffect(.25f, hitEffect));

                float distance = Vector3.Distance(weaponCam.transform.position, hit.point);
                damageDivider = distance < dropoffRange ? 1 : dropOffDivider;

                string entityTag = hit.collider.tag;
                BoltEntity boltEntity = hit.collider.GetComponent<BoltEntity>();
                if (!boltEntity) { boltEntity = hit.collider.GetComponentInParent<BoltEntity>(); }

                if (boltEntity)
                {
                    int damageToDo = damage / damageDivider;
                    if (hit.collider.GetComponent<Health>() && hit.collider.GetComponent<EnemyHealth>() && hit.collider.GetComponentInParent<EnemyHealth>())
                    {
                        totalDamage += 0;
                    }
                    else if (entityTag == "Enemy")
                    {
                        SendDamage(damageToDo, true, boltEntity);
                        totalDamage += damageToDo;
                    }
                    else if (entityTag == "EnemyHead")
                    {
                        SendDamage(damageToDo * hsMultiplier, true, boltEntity);
                        totalDamage += damageToDo * hsMultiplier;
                    }
                    else if (entityTag == enemyTeamTag)
                    {
                        SendDamage(damageToDo, false, boltEntity);
                        totalDamage += damageToDo;
                    }
                    else if (boltEntity.GetComponentInChildren<Health>().CompareTag(enemyTeamTag))
                    {
                        SendDamage(damageToDo * hsMultiplier, false, boltEntity);
                        totalDamage += damageToDo * hsMultiplier;
                    }
                }
            }
            if (amountShot == shotgunPellets)
            {
                if (totalDamage != 0)
                {
                    hitDamageUI.SendDamage(0, true, totalDamage);
                }
            }
        }   
    }

    public void ThrowKnives()
    {
        StartCoroutine(DisableShooting(.5f));

        foreach(Transform point in throwPoints)
        {
            var kniev = BoltNetwork.Instantiate(knife, point.position, point.rotation);
            kniev.GetComponent<ThrowingKnife>().SetTags(teamTag, enemyTeamTag);
            kniev.GetComponent<Rigidbody>().AddRelativeForce(0, 0, force, ForceMode.Impulse);
        }
    }

    void SendDamage(int damage, bool isEnemy, BoltEntity entityShot)
    {
        var request = DamageRequest.Create();
        request.EntityShot = entityShot;
        request.Damage = damage;
        request.IsEnemy = isEnemy;
        request.EntityShooter = entity;
        request.Send();
    }

    void InstantiateEffect()
    {
        var Effect = BoltNetwork.Instantiate(flash, muzzle.position, muzzle.rotation);
        Effect.GetComponent<BoltEntity>().transform.SetParent(muzzle);
        StartCoroutine(DestroyEffect(.1f, Effect));
    }

    public IEnumerator DestroyEffect(float time, BoltEntity entity)
    {
        yield return new WaitForSeconds(time);

        BoltNetwork.Destroy(entity);

        StopCoroutine(nameof(DestroyEffect));
    }

    public IEnumerator Reload(float time)
    {
        reloadingText.SetActive(true);

        yield return new WaitForSeconds(time);

        ResetAmmo();
        nextShot = false;
        reloading = false;

        reloadingText.SetActive(false);

        StopCoroutine(nameof(Reload));
    }

    public IEnumerator DisableShooting(float time)
    {
        shootingDisabled = true;

        yield return new WaitForSeconds(time);

        shootingDisabled = false;

        StopCoroutine(nameof(DisableShooting));
    }

    public void ResetAmmo()
    {
        currentBulletCount = maxBulletCount;
    }

    public void ReduceSpread(bool crouching)
    {
        if (crouching)
            randomSpread /= spreadDivider;
        else
            randomSpread *= spreadDivider;
    }

    public void SetTags()
    {
        teamTag = GetComponentInParent<Health>().tag;
        if (teamTag == "Team1") { enemyTeamTag = "Team2"; }
        else if (teamTag == "Team2") { enemyTeamTag = "Team1"; }
    }
}
