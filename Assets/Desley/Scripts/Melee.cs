using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] Animator animator;
    [SerializeField] Transform shankPoint, player;
    [SerializeField] int damage;
    [SerializeField] float shankRate, range, damageTime;
    float nextTimeToShank;

    public Collider[] hitObjects;
    public List<GameObject> hitEnemies = new List<GameObject>();
    public GameObject enemyToAttack;
    public float closestDistanceToScreen = Mathf.Infinity;

    bool isShooting;

    public void Update()
    {
        isShooting = Input.GetButtonDown("Fire1");

        if (isShooting && entity.IsOwner && Time.time >= nextTimeToShank && !state.IsDead)
        {
            state.Animator.ResetTrigger("Bonk");
            FilterEnemies();
            state.Animator.SetTrigger("Bonk");
            nextTimeToShank = Time.time + 1f / shankRate;
        }

        if (hitEnemies.Count > 0)
        {
            PredictEnemy();
        }
    }

    public override void Attached()
    {
        state.SetAnimator(animator);
    }

    public void FilterEnemies()
    {
        hitObjects = Physics.OverlapSphere(shankPoint.position, range);
        foreach (Collider collider in hitObjects)
        {
            if (collider.GetComponent<EnemyHealth>() && !hitEnemies.Contains(collider.gameObject))
            {
                hitEnemies.Add(collider.gameObject);
            }
        }
    }

    public void PredictEnemy()
    {
        foreach(GameObject enemy in hitEnemies.ToArray())
        {
            float DistanceToScreen = Vector3.Dot(player.position, enemy.transform.position.normalized);
            if(DistanceToScreen <= closestDistanceToScreen)
            {
                closestDistanceToScreen = DistanceToScreen;
                enemyToAttack = enemy;
            }
            hitEnemies.Remove(enemy);
        }

        if (hitEnemies.Count == 0)
        {
            StartCoroutine(DoDamage(damageTime));
            closestDistanceToScreen = Mathf.Infinity;
        }
    }

    public IEnumerator DoDamage(float time)
    {
        yield return new WaitForSeconds(time);

        //Create DamageRequest, set entity to ent and Damage to damage, then send
        var request = DamageRequest.Create();
        request.Entity = enemyToAttack.GetComponent<BoltEntity>();
        request.Damage = damage;
        request.IsEnemy = true;
        request.Send();
        enemyToAttack = null;
        StopCoroutine(nameof(DoDamage));
    }
}
