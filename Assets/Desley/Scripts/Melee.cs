using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] Camera cam;
    [SerializeField] Animator animator;
    [SerializeField] HitDamageUI hitDamageUI;
    [SerializeField] Transform shankPoint;
    [SerializeField] int damage;
    [SerializeField] float shankRate, range, damageTime;
    float nextTimeToShank;

    Collider[] hitObjects;
    List<GameObject> hitEnemies = new List<GameObject>();
    GameObject enemyToAttack;
    float closestDistanceToScreen = Mathf.Infinity;

    bool isShooting;

    public void Update()
    {
        isShooting = Input.GetButton("Fire1");

        if (isShooting && entity.IsOwner && Time.time >= nextTimeToShank && !state.IsDead)
        {
            state.Animator.ResetTrigger("Bonk");
            StartCoroutine(DoDamage(damageTime));
            state.Animator.SetTrigger("Bonk");
            nextTimeToShank = Time.time + 1f / shankRate;
        }
    }

    public override void Attached()
    {
        state.SetAnimator(animator);

        if (entity.IsOwner)
        {
            int index = 9;
            gameObject.layer = index;
            var transforms = GetComponentsInChildren<Transform>();

            foreach (var gameObject in transforms)
            {
                gameObject.gameObject.layer = index;
            }
        }
    }

    public void Shank()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range))
        {
            if(hit.collider.GetComponent<EnemyHealth>() && entity.IsOwner)
            {
                enemyToAttack = hit.collider.gameObject;

                //Create DamageRequest, set entity to ent and Damage to damage, then send
                var request = DamageRequest.Create();
                request.EntityShot = enemyToAttack.GetComponent<BoltEntity>();
                request.Damage = damage;
                request.IsEnemy = true;
                request.EntityShooter = entity;
                request.Send();
                hitDamageUI.SendDamage(damage, true);
                enemyToAttack = null;
            }

            if (hit.collider.GetComponent<Health>() && entity.IsOwner)
            {
                enemyToAttack = hit.collider.gameObject;

                //Create DamageRequest, set entity to ent and Damage to damage, then send
                var request = DamageRequest.Create();
                request.EntityShot = enemyToAttack.GetComponentInParent<BoltEntity>();
                request.Damage = damage;
                request.EntityShooter = entity;
                request.Send();
                hitDamageUI.SendDamage(damage, true);
                enemyToAttack = null;
            }
        }
    }

    public IEnumerator DoDamage(float time)
    {
        yield return new WaitForSeconds(time);

        Shank();

        StopCoroutine(nameof(DoDamage));
    }
}
