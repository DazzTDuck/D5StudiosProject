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
    [SerializeField] GunSounds gunSounds;

    [Space, SerializeField] GameObject BulletCountCanvas;
    [SerializeField] TMP_Text bulletCountText;
    [SerializeField] GameObject reloadingText;

    [Space, SerializeField] GameObject flash;
    [SerializeField] GameObject bulletHit;
    [SerializeField] Transform muzzle;

    [Space, SerializeField] int damage, powerUp, hsMultiplier;
    [SerializeField] float fireRate, weaponPunchY;
    [SerializeField] float[] weaponPunchX;

    [Space, SerializeField] int currentBulletCount;
    [SerializeField] int maxBulletCount;
    [SerializeField] float reloadTime;
    bool reloading;
    
    [Space, SerializeField] Vector3[] sprayPattern;
    [SerializeField] int sprayPatternIndex;
    [SerializeField] float recoilResetAddTime;
    float recoilResetTime;
    int count;

    [Space, SerializeField] GameObject clusterBomb;
    [SerializeField] Transform throwPoint;
    [SerializeField] float throwForce;

    float nextTimeToShoot;
    bool isShooting;
    bool nextShot;
    bool fullAuto = true;

    bool accuracyStimmed;

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
                nextShot = false;

                if (!accuracyStimmed)
                {
                    weaponCam.GetComponent<PlayerCamera>().AddRecoil(weaponPunchY, weaponPunchX[sprayPatternIndex - 1]);
                    cam.GetComponent<PlayerCamera>().AddRecoil(weaponPunchY, weaponPunchX[sprayPatternIndex - 1]);
                }
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
        if (!state.IsStunned)
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

    public override void Attached()
    {
        state.SetAnimator(animator);
        state.AddCallback("IsPoweredUp", DamageCallback);

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

    void DamageCallback() { damage = state.IsPoweredUp ? damage *= powerUp : damage /= powerUp; }

    public void ShootRaycast()
    {
        state.Animator.SetTrigger("Shoot");
        gunSounds.PlaySound("Fire");
        Vector3 ray = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.5f) + sprayPattern[sprayPatternIndex-1]);
        RaycastHit hit;
        if(Physics.Raycast(ray, cam.transform.forward, out hit))
        {
            var hitEffect = BoltNetwork.Instantiate(bulletHit, hit.point, Quaternion.identity);
            StartCoroutine(DestroyEffect(.25f, hitEffect));

            string entityTag = hit.collider.tag;
            BoltEntity boltEntity = hit.collider.GetComponentInParent<BoltEntity>();

            if (boltEntity)
            {
                if (entityTag == enemyTeamTag)
                {
                    SendDamage(damage, boltEntity);
                }
                else if (boltEntity.GetComponentInChildren<Health>().CompareTag(enemyTeamTag))
                {
                    SendDamage(damage * hsMultiplier, boltEntity);
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
        gunSounds.PlaySound("Reload");

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
        accuracyStimmed = true;

        foreach (Vector3 pattern in sprayPattern)
        {
            pattern.Set(pattern.x / 10, pattern.y / 10, pattern.z / 10);
            sprayPattern[count] = pattern;
            count++;
        }

        yield return new WaitForSeconds(time);

        count = 0;
        accuracyStimmed = false;

        foreach (Vector3 pattern in sprayPattern)
        {
            pattern.Set(pattern.x * 10, pattern.y * 10, pattern.z * 10);
            sprayPattern[count] = pattern;
            count++;
        }

        sprayPatternIndex = 0;

        StopCoroutine(nameof(AccuracyStim));
    }

    public void ClusterFuck()
    {
        var bomb = BoltNetwork.Instantiate(clusterBomb, throwPoint.position, throwPoint.rotation);
        bomb.GetComponent<DetonateBomb>().SetTags(teamTag, enemyTeamTag);
        bomb.GetComponent<Rigidbody>().AddRelativeForce(0, 0, throwForce, ForceMode.Impulse);
    }

    public void SetTags()
    {
        teamTag = GetComponentInParent<Health>().tag;
        if (teamTag == "Team1") { enemyTeamTag = "Team2"; }
        else if (teamTag == "Team2") { enemyTeamTag = "Team1"; }
    }
}
