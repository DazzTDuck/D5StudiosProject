using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombFragment : Bolt.EntityBehaviour<IProjectileState>
{
    [SerializeField] GameObject explosionEffect;
    [SerializeField] int damage;
    [SerializeField] float explosionTime;
    [SerializeField] float radius;
    [SerializeField] float effectDestroyTime;

    [Space, SerializeField] List<BoltEntity> entitiesHit;

    string teamTag;
    string enemyTeamTag;

    public override void Attached()
    {
        state.SetTransforms(state.ProjectileTransform, transform);
    }

    IEnumerator ExplodeFragment(float time)
    {
        yield return new WaitForSeconds(time);

        GetHitObjects();

        StopCoroutine(nameof(ExplodeFragment));
    }

    void GetHitObjects()
    {
        Collider[] hitObjects = Physics.OverlapSphere(transform.position, radius);
        foreach(Collider obj in hitObjects)
        {
            if (obj.gameObject.layer == 15)
                return;

            BoltEntity entity = obj.GetComponentInParent<BoltEntity>();

            if (entity && !entitiesHit.Contains(entity))
            {
                Health health = entity.GetComponentInChildren<Health>();

                if (health && health.CompareTag(enemyTeamTag))
                {
                    entitiesHit.Add(entity);
                }
            }
        }

        SendDamage();
    }

    void SendDamage()
    {
        if(entitiesHit.Count != 0)
        {
            foreach (BoltEntity entity in entitiesHit)
            {
                var request = DamageRequest.Create();
                request.EntityShot = entity;
                request.Damage = damage;
                request.EntityShooter = GetComponentInParent<BoltEntity>();
                request.Send();
            }
        }

        var effect = BoltNetwork.Instantiate(explosionEffect, transform.position, Quaternion.identity);
        StartCoroutine(DestroyEffect(effect, effectDestroyTime));
    }

    IEnumerator DestroyEffect(BoltEntity effect, float time)
    {
        yield return new WaitForSeconds(time);

        BoltNetwork.Destroy(effect);
        DestroyFragment();

        StopCoroutine(nameof(DestroyEffect));
    }

    void DestroyFragment()
    {
        BoltNetwork.Destroy(gameObject);
    }

    public void SetTags(string team, string enemyTeam)
    {
        teamTag = team;
        enemyTeamTag = enemyTeam;

        StartCoroutine(ExplodeFragment(explosionTime));
    }
}
