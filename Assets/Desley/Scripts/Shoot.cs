using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Shoot : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] Camera cam, weaponCam;
    [SerializeField] GameObject flash;
    [SerializeField] GameObject bulletHit;
    [SerializeField] Transform muzzle;
    [SerializeField] int damage;
    [SerializeField] float fireRate, weaponPunch;
    [SerializeField] Animator animator;

    [Space, SerializeField] GameObject BulletCountCanvas;
    [SerializeField] TMP_Text bulletCountText;
    [SerializeField] GameObject reloadingText;

    [Space, SerializeField] int currentBulletCount;
    [SerializeField] int maxBulletCount;
    [SerializeField] float reloadTime;
    [SerializeField] bool reloading;

    float nextTimeToShoot;
    bool isShooting;
    bool nextShot;

    EnemyHealth enemyHealth;
    Health health;

    private void Update()
    {
        isShooting = Input.GetButtonDown("Fire1") && nextTimeToShoot < Time.time;

        if (isShooting && entity.IsOwner && currentBulletCount > 0 && !reloading && Time.time >= nextTimeToShoot && !state.IsDead || nextShot && entity.IsOwner && currentBulletCount > 0 && !reloading && Time.time >= nextTimeToShoot && !state.IsDead)
        {
            state.Animator.ResetTrigger("Shoot");
            ShootRaycast();
            InstantiateEffect();
            weaponCam.GetComponent<PlayerCamera>().AddRecoil(weaponPunch);
            cam.GetComponent<PlayerCamera>().AddRecoil(weaponPunch);
            nextTimeToShoot = Time.time + 1f / fireRate;
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
        Ray ray = weaponCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            var hitEffect = BoltNetwork.Instantiate(bulletHit, hit.point, Quaternion.identity);
            StartCoroutine(DestroyEffect(0.25f, hitEffect));

            enemyHealth = hit.collider.gameObject.GetComponent<EnemyHealth>();
            if (enemyHealth && entity.IsOwner)
            {
                //Create DamageRequest, set entity to ent and Damage to damage, then send
                var request = DamageRequest.Create();
                request.Entity = enemyHealth.GetComponent<BoltEntity>();
                request.Damage = damage;
                request.IsEnemy = true;
                request.Send();
                enemyHealth = null;
            }
            health = hit.collider.gameObject.GetComponent<Health>();
            if (health && entity.IsOwner)
            {
                //Create DamageRequest, set entity to ent and Damage to damage, then send
                var request = DamageRequest.Create();
                request.Entity = health.GetComponentInParent<BoltEntity>();
                request.Damage = damage;
                request.Send();
                enemyHealth = null;
            }
        }
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
}
