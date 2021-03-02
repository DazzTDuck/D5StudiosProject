using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombFragment : Bolt.EntityBehaviour<IProjectileState>
{
    [SerializeField] int damage;
    [SerializeField] float explosionTime;
    [SerializeField] float radius;

    [Space, SerializeField] List<BoltEntity> entitiesHit;

    string teamTag;
    string enemyTeamTag;

    public override void Attached()
    {
        state.SetTransforms(state.ProjectileTransform, transform);
    }

    void Start()
    {
        StartCoroutine(ExplodeFragment(explosionTime));
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
            BoltEntity entity = obj.GetComponentInParent<BoltEntity>();

            if (entity && !entitiesHit.Contains(entity))
            {
                if (obj.CompareTag(enemyTeamTag))
                {
                    entitiesHit.Add(entity);
                }
                else if (obj.GetComponentInParent<Health>() && obj.GetComponentInParent<Health>().CompareTag(enemyTeamTag))
                {
                    entitiesHit.Add(entity);
                }
            }
        }

        if (entitiesHit.Count != 0)
            SendDamage();
        else
            DestroyFragment();
    }

    void SendDamage()
    {
        foreach(BoltEntity entity in entitiesHit)
        {
            var request = DamageRequest.Create();
            request.EntityShot = entity;
            request.Damage = damage;
            request.EntityShooter = GetComponentInParent<BoltEntity>();
            request.Send();
        }

        DestroyFragment();
    }

    void DestroyFragment()
    {
        BoltNetwork.Destroy(gameObject);
    }

    public void SetTags(string team, string enemyTeam)
    {
        teamTag = team;
        enemyTeamTag = enemyTeam;
    }
}
