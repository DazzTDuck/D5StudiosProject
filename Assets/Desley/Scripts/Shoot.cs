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
    [SerializeField] PlayPlayerSounds playerSounds;
    [SerializeField] AbilityHandler abilityHandler;

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

    [Space, SerializeField] float stimAnimationTimer = .5f;
    [SerializeField] float clusterAnimationTimer = .5f;
    [SerializeField] int healing;
    [SerializeField] float accuracyStimTime;
    bool accuracyStimmed;

    [Space, SerializeField] GameObject clusterBomb;
    [SerializeField] Transform throwPoint;
    [SerializeField] float throwForce;

    float nextTimeToShoot;
    bool isShooting;
    bool nextShot;
    bool fullAuto = true;

    [Space, SerializeField] float disableShootingTimeStim = .5f;
    [SerializeField] float disableShootingTimeNade = .5f;

    [Space, SerializeField] int bleedDamage;
    [SerializeField] int bleedTimes;
    [SerializeField] float timeInBetween;

    string teamTag;
    string enemyTeamTag;

    [SerializeField] LayerMask ignoreLayer;

    public override void Attached()
    {
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

    private void Update()
    {
        if (!state.IsUsingAbility && !state.IsStunned)
        {
            CheckFireModeInput();
            CheckReloadInput();
        }

        if (entity.IsOwner && currentBulletCount > 0 && !reloading && Time.time >= nextTimeToShoot && !state.IsDead)
        {
            if (isShooting || nextShot)
            {
                currentBulletCount--;
                sprayPatternIndex++;
                animator.ResetTrigger("Shoot");
                animator.ResetTrigger("Stim");
                animator.ResetTrigger("Nade");
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
            if (!fullAuto)
                isShooting = Input.GetButtonDown("Fire1") && nextTimeToShoot < Time.time && !pauseMenuHandler.GetIfPaused();
            else
                isShooting = Input.GetButton("Fire1") && nextTimeToShoot < Time.time && !pauseMenuHandler.GetIfPaused();

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
            animator.SetTrigger("Reload");
            StartCoroutine(Reload(reloadTime));
            reloading = true;
        }
        if (!reloading)
            bulletCountText.text = currentBulletCount + "|" + maxBulletCount;
    }

    void DamageCallback() { damage = state.IsPoweredUp ? damage *= powerUp : damage /= powerUp; }

    public void ShootRaycast()
    {
        animator.SetTrigger("Shoot");
        playerSounds.PlaySoundRequest(0);

        Vector3 ray = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.5f));
        var dir = cam.transform.forward + sprayPattern[sprayPatternIndex - 1];
        RaycastHit hit;
        if(Physics.Raycast(ray, dir, out hit, ignoreLayer))
        {
            var hitEffect = BoltNetwork.Instantiate(bulletHit, hit.point, Quaternion.identity);
            StartCoroutine(DestroyEffect(.25f, hitEffect));

            string entityTag = hit.collider.tag;
            BoltEntity boltEntity = hit.collider.GetComponentInParent<BoltEntity>();

            if (boltEntity)
            {
                Health health = boltEntity.GetComponentInChildren<Health>();

                if (health)
                {
                    if (entityTag == "PlayerHead" && health.CompareTag(enemyTeamTag))
                    {
                        SendDamage(damage * hsMultiplier, boltEntity);
                    }
                    else if (health.CompareTag(enemyTeamTag))
                    {
                        SendDamage(damage, boltEntity);
                    }
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

        if (state.CanBleedEnemies)
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

    void InstantiateEffect()
    {
        var Effect = BoltNetwork.Instantiate(flash, muzzle.position, muzzle.rotation);
        Effect.GetComponent<BoltEntity>().transform.SetParent(muzzle);
        StartCoroutine(DestroyEffect(.1f, Effect));
    }

    IEnumerator DestroyEffect(float time, BoltEntity entity)
    {
        yield return new WaitForSeconds(time);

        BoltNetwork.Destroy(entity);

        StopCoroutine(nameof(DestroyEffect));
    }

    IEnumerator Reload(float time)
    {
        reloadingText.SetActive(true);
        playerSounds.PlaySoundRequest(1);

        yield return new WaitForSeconds(time);

        nextShot = false;
        reloading = false;

        reloadingText.SetActive(false);
        animator.ResetTrigger("Reload");

        if (state.IsUsingAbility)
            yield break;

        ResetAmmo();

        StopCoroutine(nameof(Reload));
    }

    public void ResetAmmo()
    {
        currentBulletCount = maxBulletCount;
    }

    public void CrouchRecoil(bool crouching, float divider)
    {
        count = 0;
        if (crouching)
        {
            foreach (Vector3 pattern in sprayPattern)
            {
                pattern.Set(pattern.x / divider, pattern.y / divider, pattern.z / divider);
                sprayPattern[count] = pattern;
                count++;
            }
        }
        else
        {
            foreach (Vector3 pattern in sprayPattern)
            {
                pattern.Set(pattern.x * divider, pattern.y * divider, pattern.z * divider);
                sprayPattern[count] = pattern;
                count++;
            }
        }
    }

    public void StartAnimation(int index) 
    {
        if (state.IsUsingAbility)
        {
            if (index == 0)
                abilityHandler.ResetAbility2Timer();
            else if (index == 1)
                abilityHandler.ResetAbility1Timer();

            return;
        }

        if(index == 0 || index == 1)
            StartCoroutine(WaitForAnimation(index, stimAnimationTimer)); 
        else
            StartCoroutine(WaitForAnimation(index, clusterAnimationTimer));
    }

    IEnumerator WaitForAnimation(int index, float time)
    {
        var timeDisableShooting = 0f;

        if(index == 0)
        {
            animator.SetTrigger("Stim");
            timeDisableShooting = disableShootingTimeStim;
            abilityHandler.ultimateActivatable = false;
        }            
        else if(index == 1)
        {
            animator.SetTrigger("Stim");
            timeDisableShooting = disableShootingTimeStim;
            abilityHandler.ultimateActivatable = false;
        }           
        else if(index == 2)
        {
            animator.SetTrigger("Nade");
            timeDisableShooting = disableShootingTimeNade;
        }

        StartCoroutine(DisableShooting(timeDisableShooting));

        yield return new WaitForSeconds(time);

        if (state.StopAbilities)
            yield break;

        if (index == 0)
            HealingStim();
        else if (index == 1)
            StartCoroutine(AccuracyStim(accuracyStimTime));
        else if (index == 2)
            ClusterFuck();

        StopCoroutine(nameof(WaitForAnimation));
    }

    //start animation index = 0
    void HealingStim()
    {
        var request = HealRequest.Create();
        request.EntityShot = GetComponentInParent<BoltEntity>();
        request.Healing = healing;
        request.HealStim = true;
        request.Send();
    }

    //start animation index = 1
    IEnumerator AccuracyStim(float time)
    {
        animatorOverlay.ResetTrigger("Accuracy");
        animatorOverlay.SetTrigger("Accuracy");

        accuracyStimmed = true;
        GetComponentInParent<PlayerController>().isStimmed = true;
        CrouchRecoil(true, 10);

        yield return new WaitForSeconds(time);

        accuracyStimmed = false;
        GetComponentInParent<PlayerController>().isStimmed = false;
        CrouchRecoil(false, 10);

        sprayPatternIndex = 0;

        StopCoroutine(nameof(AccuracyStim));
    }

    //start animation index = 2
    void ClusterFuck()
    {
        var bomb = BoltNetwork.Instantiate(clusterBomb, throwPoint.position, throwPoint.rotation);
        bomb.GetComponent<DetonateBomb>().SetTags(teamTag, enemyTeamTag);
        bomb.GetComponent<Rigidbody>().AddRelativeForce(0, 0, throwForce, ForceMode.Impulse);
    }

    IEnumerator DisableShooting(float time)
    {
        state.IsUsingAbility = true;

        yield return new WaitForSeconds(time);

        state.IsUsingAbility = false;
        abilityHandler.ultimateActivatable = true;

        StopCoroutine(nameof(DisableShooting));
    }

    public void SetTags()
    {
        teamTag = GetComponentInParent<Health>().tag;
        if (teamTag == "Team1") { enemyTeamTag = "Team2"; }
        else if (teamTag == "Team2") { enemyTeamTag = "Team1"; }
    }
}
