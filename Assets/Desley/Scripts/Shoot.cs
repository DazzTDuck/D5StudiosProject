﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Shoot : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] Camera cam, weaponCam;
    [SerializeField] GameObject flash;
    [SerializeField] GameObject bulletHit;
    [SerializeField] Transform muzzle;
    [SerializeField] float[] weaponPunchY;
    [SerializeField] Animator animator;

    [Space, SerializeField] GameObject BulletCountCanvas;
    [SerializeField] TMP_Text bulletCountText;
    [SerializeField] GameObject reloadingText;

    [Space, SerializeField] int damage, hsMultiplier;
    [SerializeField] float fireRate, weaponPunchX;

    [Space, SerializeField] int currentBulletCount;
    [SerializeField] int maxBulletCount;
    [SerializeField] float reloadTime;
    [SerializeField] bool reloading;
    
    [Space, SerializeField] Vector3[] sprayPattern;
    [SerializeField] int sprayPatternIndex;
    [SerializeField] float recoilResetAddTime;
    float recoilResetTime;
    int count;

    float nextTimeToShoot;
    bool isShooting;
    bool nextShot;
    bool fullAuto;

    EnemyHealth enemyHealth;
    Health health;

    private void Update()
    {
        if(Input.GetButtonDown("FireMode") && !fullAuto) { fullAuto = true; } 
        else if(Input.GetButtonDown("FireMode") && fullAuto) { fullAuto = false; }

        if(!fullAuto)
            isShooting = Input.GetButtonDown("Fire1") && nextTimeToShoot < Time.time;
        else
            isShooting = Input.GetButton("Fire1") && nextTimeToShoot < Time.time;

        if (isShooting && entity.IsOwner && currentBulletCount > 0 && !reloading && Time.time >= nextTimeToShoot && !state.IsDead || nextShot && entity.IsOwner && currentBulletCount > 0 && !reloading && Time.time >= nextTimeToShoot && !state.IsDead)
        {
            state.Animator.ResetTrigger("Shoot");
            ShootRaycast();
            InstantiateEffect();
            recoilResetTime = recoilResetAddTime;
            nextTimeToShoot = Time.time + 1f / fireRate;
            weaponCam.GetComponent<PlayerCamera>().AddRecoil(weaponPunchX, weaponPunchY[sprayPatternIndex - 1]);
            cam.GetComponent<PlayerCamera>().AddRecoil(weaponPunchX, weaponPunchY[sprayPatternIndex - 1]);
            nextShot = false;
            currentBulletCount--;
        }

        if (nextTimeToShoot > Time.time && Input.GetButtonDown("Fire1") && !isShooting)
            nextShot = true;
        else if (nextTimeToShoot > Time.time && Input.GetButtonUp("Fire1"))
            nextShot = false;

        if(Input.GetButtonDown("Reload") && !reloading && nextTimeToShoot < Time.time && currentBulletCount != maxBulletCount || currentBulletCount == 0 && !reloading && nextTimeToShoot < Time.time)
        {
            StartCoroutine(Reload(reloadTime));
            reloading = true;
        }
        if (!reloading)
            bulletCountText.text = currentBulletCount + "|" + maxBulletCount;

        recoilResetTime -= Time.deltaTime;
        if (recoilResetTime <= 0)
        {
            sprayPatternIndex = 0;
            recoilResetTime = Mathf.Infinity;
        }
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
        state.Animator.SetTrigger("Shoot");
        Ray ray = weaponCam.ScreenPointToRay(Input.mousePosition + sprayPattern[sprayPatternIndex]);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            sprayPatternIndex++;
            var hitEffect = BoltNetwork.Instantiate(bulletHit, hit.point, Quaternion.identity);
            StartCoroutine(DestroyEffect(0.25f, hitEffect));

            //headshot or bodyshot
            enemyHealth = hit.collider.gameObject.GetComponentInParent<EnemyHealth>();
            if(!enemyHealth)
                enemyHealth = hit.collider.gameObject.GetComponent<EnemyHealth>();

            if (enemyHealth && hit.collider.CompareTag("enemyHead") && entity.IsOwner)
            {
                SendDamage(damage * hsMultiplier, true, enemyHealth.GetComponent<BoltEntity>());
            }
            else if (enemyHealth && hit.collider.CompareTag("enemy") && entity.IsOwner)
            {
                SendDamage(damage, true, enemyHealth.GetComponent<BoltEntity>());
            }
          
            health = hit.collider.gameObject.GetComponent<Health>();
            if (health && entity.IsOwner)
            {
                SendDamage(damage, false, health.GetComponentInParent<BoltEntity>());
            }
        }
    }

    void SendDamage(int damage, bool isEnemy, BoltEntity entityShot)
    {
        var request = DamageRequest.Create();
        request.EntityShot = entityShot;
        request.Damage = damage;
        request.IsEnemy = isEnemy;
        request.EntityShooter = entity;
        request.ShowDamage = true;
        request.Send();
        if(isEnemy) { enemyHealth = null; } else { health = null; }
    }

    public void InstantiateEffect()
    {
        var flashEffect = BoltNetwork.Instantiate(flash, muzzle.position, muzzle.rotation);
        flashEffect.GetComponent<BoltEntity>().transform.SetParent(muzzle);
        StartCoroutine(DestroyEffect(0.1f, flashEffect));
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

        currentBulletCount = maxBulletCount;
        nextShot = false;
        reloading = false;

        reloadingText.SetActive(false);

        StopCoroutine(nameof(Reload));
    }

    public void ResetAmmo()
    {
        currentBulletCount = maxBulletCount;
    }

    public void ReduceRecoil(bool crouching)
    {
        count = 0;
        if (crouching)
        {
            foreach (Vector3 pattern in sprayPattern)
            {
                pattern.Set(pattern.x / 2, pattern.y / 2, pattern.z / 2);
                sprayPattern[count] = pattern;
                count++;
            }
        }
        else
        {
            foreach (Vector3 pattern in sprayPattern)
            {
                pattern.Set(pattern.x * 2, pattern.y * 2, pattern.z * 2);
                sprayPattern[count] = pattern;
                count++;
            }
        }
    }
}
