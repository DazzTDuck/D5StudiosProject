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

    public List<GameObject> hitEnemies;
    public EnemyHealth enemyHealth;
    public float closestDistanceToScreen = Mathf.Infinity;

    bool isShooting;

    public void Update()
    {
        isShooting = Input.GetButtonDown("Fire1");

        if (isShooting && entity.IsOwner && Time.time >= nextTimeToShank && !state.IsDead)
        {
            state.Animator.ResetTrigger("Bonk");
            FilterEnemies();
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
        Collider[] hitObjects = Physics.OverlapSphere(shankPoint.position, range);
        foreach (Collider collider in hitObjects)
        {
            if (collider.GetComponent<EnemyHealth>() && !hitEnemies.Contains(collider.gameObject))
            {
                hitEnemies.Add(collider.gameObject);
            }
            else if (collider.GetComponentInParent<EnemyHealth>() && !hitEnemies.Contains(collider.gameObject))
            {
                hitEnemies.Add(collider.GetComponentInParent<Collider>().gameObject);
            }
        }
    }

    public void PredictEnemy()
    {
        foreach(GameObject enemy in hitEnemies)
        {
            float DistanceToScreen = Vector3.Dot(cam.transform.position, enemy.transform.position.normalized);
            if(DistanceToScreen <= closestDistanceToScreen)
            {
                closestDistanceToScreen = DistanceToScreen;
                enemyHealth = enemy.GetComponent<EnemyHealth>();
            }
            hitEnemies.Remove(enemy);
        }

        if (hitEnemies.Count == 0)
        {
            Shank();
            closestDistanceToScreen = Mathf.Infinity;
        }
    }

    public void Shank()
    {
        //shank erin, shank eruit
        state.Animator.SetTrigger("Bonk");
        StartCoroutine(DoDamage(damageTime));
    }

    public IEnumerator DoDamage(float time)
    {
        yield return new WaitForSeconds(time);

        //Create DamageRequest, set entity to ent and Damage to damage, then send
        var request = DamageRequest.Create();
        request.Entity = enemyHealth.GetComponent<BoltEntity>();
        request.Damage = damage;
        request.IsEnemy = true;
        request.Send();
        enemyHealth = null;
        StopCoroutine(nameof(DoDamage));
    }
}
