using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] Camera cam, weaponCam;
    [SerializeField] GameObject flash;
    [SerializeField] GameObject bulletHit;
    [SerializeField] Transform muzzle;
    [SerializeField] int damage;
    [SerializeField] float fireRate, weaponPunch;
    [SerializeField] Animator animator;
    float nextTimeToShoot;

    //Health health;
    EnemyHealth enemyHealth;

    bool isShooting;

    private void Update()
    {
        isShooting = Input.GetButton("Fire1");

        if (isShooting && entity.IsOwner && Time.time >= nextTimeToShoot && !state.IsDead)
        {
            state.Animator.ResetTrigger("Shoot");
            ShootRaycast();
            cam.GetComponent<PlayerCamera>().AddRecoil(weaponPunch);
            weaponCam.GetComponent<PlayerCamera>().AddRecoil(weaponPunch);
            var flashEffect = BoltNetwork.Instantiate(flash, muzzle.position, muzzle.rotation);
            flashEffect.GetComponent<BoltEntity>().transform.SetParent(muzzle);
            StartCoroutine(DestroyEffect(0.1f, flashEffect));
            nextTimeToShoot = Time.time + 1f / fireRate;
        }
    }

    public override void Attached()
    {
        base.Attached();
        state.SetAnimator(animator);
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

            /*kill player
            health = hit.collider.gameObject.GetComponent<Health>();
            if (health && !state.IsDead)
            {
                //Create DamageRequest, set entity to ent and Damage to damage, then send
                var request = DamageRequest.Create();
                request.Entity = health.GetComponentInParent<BoltEntity>();
                request.Damage = damage;
                request.Send();
                health = null;
            }
            */
            
            //kill enemies
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
        }
    }

    public IEnumerator DestroyEffect(float time, BoltEntity entity)
    {
        yield return new WaitForSeconds(time);

        BoltNetwork.Destroy(entity);

        StopCoroutine(nameof(DestroyEffect));
    }
}
