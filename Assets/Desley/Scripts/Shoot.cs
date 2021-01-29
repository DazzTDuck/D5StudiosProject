using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] Camera cam;
    [SerializeField] GameObject flash;
    [SerializeField] GameObject bulletHit;
    [SerializeField] Transform muzzle;
    [SerializeField] int damage, range;
    [SerializeField] float fireRate;
    [SerializeField] Animator animator;
    float nextTimeToShoot;

    Health health;
    EnemyHealth enemyHealth;

    bool isShooting;

    private void Update()
    {
        isShooting = Input.GetButtonDown("Fire1");

        if (isShooting && entity.IsOwner && Time.time >= nextTimeToShoot && !state.IsDead)
        {
            state.Animator.ResetTrigger("Shoot");
            //state.WeaponTrigger();
            ShootRaycast();
            var flashEffect = BoltNetwork.Instantiate(flash, muzzle.position, muzzle.rotation);
            flashEffect.GetComponent<BoltEntity>().transform.SetParent(muzzle);
            StartCoroutine(DestroyEffect(0.1f, flashEffect));
            nextTimeToShoot = Time.time + 1f / fireRate;
        }
    }

    public override void Attached()
    {
        base.Attached();
        state.OnWeaponTrigger = ShootRaycast;
        state.SetAnimator(animator);
    }

    public void ShootRaycast()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            state.Animator.SetTrigger("Shoot");
            var hitEffect = BoltNetwork.Instantiate(bulletHit, hit.point, Quaternion.identity);
            StartCoroutine(DestroyEffect(0.25f, hitEffect));

            health = hit.collider.gameObject.GetComponent<Health>();
            if (health)
            {
                //Create DamageRequest, set entity to ent and Damage to damage, then send
                var request = DamageRequest.Create();
                request.Entity = health.GetComponentInParent<BoltEntity>();
                request.Damage = damage;
                request.Send();
                health = null;
            }

            enemyHealth = hit.collider.gameObject.GetComponent<EnemyHealth>();
            if (enemyHealth)
            {
                enemyHealth.TakeDamage(damage);
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
