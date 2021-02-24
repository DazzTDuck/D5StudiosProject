using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Shoot : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [Space, SerializeField] PauseMenuHandler pauseMenuHandler;

    [SerializeField] Camera cam, weaponCam;
    [SerializeField] Animator animator;
    [SerializeField] Animator animatorOverlay;
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
    public bool isStunned;

    string teamTag;
    string enemyTeamTag;

    private void Update()
    {
        CheckFireModeInput();
        CheckReloadInput();

        if (entity.IsOwner && currentBulletCount > 0 && !reloading && Time.time >= nextTimeToShoot && !state.IsDead)
        {
            if (isShooting || nextShot)
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
        if (Input.GetButtonDown("FireMode") && !fullAuto && !pauseMenuHandler.GetIfPaused()) { fullAuto = true; }
        else if (Input.GetButtonDown("FireMode") && fullAuto && !pauseMenuHandler.GetIfPaused()) { fullAuto = false; }

        //check input of mouse
        if (isStunned)
        {
            if (!fullAuto)
                isShooting = Input.GetButtonDown("Fire1") && nextTimeToShoot < Time.time && !pauseMenuHandler.GetIfPaused();
            else
                isShooting = Input.GetButton("Fire1") && nextTimeToShoot < Time.time && !pauseMenuHandler.GetIfPaused();
        }

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

            if (boltEntity)
            {
                if (entityTag == "Enemy")
                {
                    SendDamage(damage, true, boltEntity);
                }
                else if (entityTag == "EnemyHead")
                {
                    SendDamage(damage * hsMultiplier, true, boltEntity);
                }
                else if (entityTag == enemyTeamTag)
                {
                    SendDamage(damage, false, boltEntity);
                }
                else if (boltEntity.GetComponentInChildren<Health>().CompareTag(enemyTeamTag))
                {
                    SendDamage(damage * hsMultiplier, false, boltEntity);
                }
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

        ResetAmmo();
        nextShot = false;
        reloading = false;

        reloadingText.SetActive(false);

        StopCoroutine(nameof(Reload));
    }

    public void ResetAmmo()
    {
        currentBulletCount = maxBulletCount;
    }

    public void CrouchRecoil(bool crouching)
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

    public void HealingStim(int heal)
    {
        var request = HealRequest.Create();
        request.EntityShot = GetComponentInParent<BoltEntity>();
        request.Healing = heal;
        request.HealStim = true;
        request.Send();
    }

    public void StartAccuracyStim(float time) 
    {
        animatorOverlay.ResetTrigger("Accuracy");
        StartCoroutine(AccuracyStim(time));
        animatorOverlay.SetTrigger("Accuracy");
    }

    public IEnumerator AccuracyStim(float time)
    {
        count = 0;
        foreach (Vector3 pattern in sprayPattern)
        {
            pattern.Set(pattern.x / 10, pattern.y / 10, pattern.z / 10);
            sprayPattern[count] = pattern;
            count++;
        }

        yield return new WaitForSeconds(time);

        count = 0;
        foreach (Vector3 pattern in sprayPattern)
        {
            pattern.Set(pattern.x * 10, pattern.y * 10, pattern.z * 10);
            sprayPattern[count] = pattern;
            count++;
        }

        StopCoroutine(nameof(AccuracyStim));
    }

    public void SetTags()
    {
        teamTag = GetComponentInParent<Health>().tag;
        if (teamTag == "Team1") { enemyTeamTag = "Team2"; }
        else if (teamTag == "Team2") { enemyTeamTag = "Team1"; }
    }
}
