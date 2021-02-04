using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] Camera cam;
    [SerializeField] Animator animator;
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
                request.Entity = enemyToAttack.GetComponent<BoltEntity>();
                request.Damage = damage;
                request.IsEnemy = true;
                request.Send();
                enemyToAttack = null;
            }

            if (hit.collider.GetComponent<Health>() && entity.IsOwner)
            {
                enemyToAttack = hit.collider.gameObject;

                //Create DamageRequest, set entity to ent and Damage to damage, then send
                var request = DamageRequest.Create();
                request.Entity = enemyToAttack.GetComponentInParent<BoltEntity>();
                request.Damage = damage;
                request.Send();
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
