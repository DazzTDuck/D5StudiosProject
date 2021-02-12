﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Shoot : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] Camera cam, weaponCam;
    [SerializeField] Animator animator;
    [SerializeField] HitDamageUI hitDamageUI;

    [Space, SerializeField] GameObject BulletCountCanvas;
    [SerializeField] TMP_Text bulletCountText;
    [SerializeField] GameObject reloadingText;

    [Space, SerializeField] GameObject flash;
    [SerializeField] GameObject bulletHit;
    [SerializeField] Transform muzzle;

    [Space, SerializeField] int damage, hsMultiplier;
    [SerializeField] float fireRate, weaponPunchX;
    [SerializeField] float[] weaponPunchY;

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

    private void Update()
    {
        CheckFireModeInput();
        CheckReloadInput();

        if (isShooting && entity.IsOwner && currentBulletCount > 0 && !reloading && Time.time >= nextTimeToShoot && !state.IsDead || nextShot && entity.IsOwner && currentBulletCount > 0 && !reloading && Time.time >= nextTimeToShoot && !state.IsDead)
        {
            currentBulletCount--;
            sprayPatternIndex++;
            state.Animator.ResetTrigger("Shoot");
            ShootRaycast();
            InstantiateEffect();
            recoilResetTime = recoilResetAddTime;
            nextTimeToShoot = Time.time + 1f / fireRate;
            weaponCam.GetComponent<PlayerCamera>().AddRecoil(weaponPunchX, weaponPunchY[sprayPatternIndex - 1]);
            cam.GetComponent<PlayerCamera>().AddRecoil(weaponPunchX, weaponPunchY[sprayPatternIndex - 1]);
            nextShot = false;
        }

        recoilResetTime -= Time.deltaTime;
        if (recoilResetTime <= 0)
        {
            sprayPatternIndex = 0;
            recoilResetTime = Mathf.Infinity;
        }
    }

    void CheckFireModeInput()
    {
        //switch between fire modes
        if (Input.GetButtonDown("FireMode") && !fullAuto) { fullAuto = true; }
        else if (Input.GetButtonDown("FireMode") && fullAuto) { fullAuto = false; }

        //check input of mouse
        if (!fullAuto)
            isShooting = Input.GetButtonDown("Fire1") && nextTimeToShoot < Time.time;
        else
            isShooting = Input.GetButton("Fire1") && nextTimeToShoot < Time.time;

        //check for input in between chambering rounds
        if (nextTimeToShoot > Time.time && Input.GetButtonDown("Fire1") && !isShooting)
            nextShot = true;
        else if (nextTimeToShoot > Time.time && Input.GetButtonUp("Fire1"))
            nextShot = false;
    }

    void CheckReloadInput()
    {
        if (Input.GetButtonDown("Reload") && !reloading && nextTimeToShoot < Time.time && currentBulletCount != maxBulletCount || currentBulletCount == 0 && !reloading && nextTimeToShoot < Time.time)
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
        state.Animator.SetTrigger("Shoot");
        Ray ray = weaponCam.ScreenPointToRay(Input.mousePosition + sprayPattern[sprayPatternIndex - 1]);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            var hitEffect = BoltNetwork.Instantiate(bulletHit, hit.point, Quaternion.identity);
            StartCoroutine(DestroyEffect(.25f, hitEffect));

            string entityTag = hit.collider.tag;
            BoltEntity boltEntity = hit.collider.GetComponent<BoltEntity>();
            if(!boltEntity) { boltEntity = hit.collider.GetComponentInParent<BoltEntity>(); }

            if (entityTag == "Enemy" && entity.IsOwner)
            {
                SendDamage(damage, true, boltEntity);
            }
            else if (entityTag == "EnemyHead" && entity.IsOwner)
            {
                SendDamage(damage * hsMultiplier, true, boltEntity);
            }
            else if (entityTag == "Player" && entity.IsOwner)
            {
                SendDamage(damage, false, boltEntity);
            }
            else if (entityTag == "PlayerHead" && entity.IsOwner)
            {
                SendDamage(damage * hsMultiplier, false, boltEntity);
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
        request.Send();

        hitDamageUI.SendDamage(damage, true);
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
