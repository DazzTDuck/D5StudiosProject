using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Shotgun : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] Camera cam, weaponCam;
    [SerializeField] GameObject flash;
    [SerializeField] GameObject bulletHit;
    [SerializeField] Transform muzzle;
    [SerializeField] Animator animator;

    [Space, SerializeField] GameObject BulletCountCanvas;
    [SerializeField] TMP_Text bulletCountText;
    [SerializeField] GameObject reloadingText;

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

    EnemyHealth enemyHealth;
    Health health;

    int amountShot = 0;
    int overallDamage = 0;

    private void Update()
    {
        isShooting = Input.GetButtonDown("Fire1") && nextTimeToShoot < Time.time;

        if (isShooting && entity.IsOwner && currentBulletCount > 0 && !reloading && Time.time >= nextTimeToShoot && !state.IsDead || nextShot && entity.IsOwner && currentBulletCount > 0 && !reloading && Time.time >= nextTimeToShoot && !state.IsDead)
        {
            state.Animator.ResetTrigger("Shoot");
            ShootRaycast();
            InstantiateEffect();
            nextTimeToShoot = Time.time + 1f / fireRate;
            weaponCam.GetComponent<PlayerCamera>().AddRecoil(weaponPunchX, 0);
            cam.GetComponent<PlayerCamera>().AddRecoil(weaponPunchX, 0);
            nextShot = false;
            currentBulletCount--;
        }

        if (nextTimeToShoot > Time.time && Input.GetButtonDown("Fire1") && !isShooting)
            nextShot = true;
        else if (nextTimeToShoot > Time.time && Input.GetButtonUp("Fire1"))
            nextShot = false;

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

        for (var i = 0; i < shotgunPellets; i++)
        {
            var randomX = Random.Range(-randomSpread, randomSpread);
            var randomY = Random.Range(-randomSpread, randomSpread);
            spread = new Vector3(randomX, randomY, 0);

            Ray ray = weaponCam.ScreenPointToRay(Input.mousePosition + spread);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, range))
            {
                var hitEffect = BoltNetwork.Instantiate(bulletHit, hit.point, Quaternion.identity);
                StartCoroutine(DestroyEffect(0.25f, hitEffect));

                //headshot or bodyshot
                enemyHealth = hit.collider.gameObject.GetComponentInParent<EnemyHealth>();
                if (!enemyHealth)
                    enemyHealth = hit.collider.gameObject.GetComponent<EnemyHealth>();

                health = hit.collider.gameObject.GetComponent<Health>();

                if (!health && !enemyHealth && entity.IsOwner)
                {
                    CalculateAllDamage(0);                    
                }
                  
                float distance = Vector3.Distance(weaponCam.transform.position, hit.point);
                damageDivider = distance < dropoffRange ? 1 : dropOffDivider;

                if (enemyHealth && hit.collider.CompareTag("enemyHead") && entity.IsOwner)
                {
                    SendDamage(damage * hsMultiplier / damageDivider, true, enemyHealth.GetComponent<BoltEntity>());
                    CalculateAllDamage(damage * hsMultiplier / damageDivider);
                }
                else if (enemyHealth && hit.collider.CompareTag("enemy") && entity.IsOwner)
                {
                    SendDamage(damage / damageDivider, true, enemyHealth.GetComponent<BoltEntity>());
                    CalculateAllDamage(damage / damageDivider);
                }
                
                if (health && entity.IsOwner)
                {
                    SendDamage(damage / damageDivider, false, health.GetComponentInParent<BoltEntity>());
                    CalculateAllDamage(damage / damageDivider);
                }

            }
        }
    }

    void CalculateAllDamage(int addDamage)
    {
        overallDamage += addDamage;
        amountShot++;

        if (amountShot == shotgunPellets)
        {
            if(overallDamage != 0)
            {
                //send overallDamage
                var request = DamageRequest.Create();
                request.OverallDamage = overallDamage;
                request.ShowDamage = true;
                request.EntityShooter = entity;
                request.Send();
            }
            overallDamage = 0;
            amountShot = 0;
        }        
    }

    void SendDamage(int damage, bool isEnemy, BoltEntity entityShot)
    {
        var request = DamageRequest.Create();
        request.EntityShot = entityShot;
        request.Damage = damage;
        request.ShowDamage = false;
        request.IsEnemy = isEnemy;
        request.Send();
        if (isEnemy) { enemyHealth = null; } else { health = null; }
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

    public void ReduceSpread(bool crouching)
    {
        if (crouching)
            randomSpread /= spreadDivider;
        else
            randomSpread *= spreadDivider;
    }
}
