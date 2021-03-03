using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balls : Bolt.EntityBehaviour<IProjectileState>
{
    [SerializeField] int playerLayer;
    bool directHit;
    bool healBall;
    bool stunBall;

    [Space, SerializeField] int ballValue;
    [SerializeField] int directHitValue;
    [SerializeField] int powerUp;
    [SerializeField] float stunTime;
    [SerializeField] float radius;

    [Space, SerializeField] float destroyTime;

    [Space] public BoltEntity playerEntity;
    [SerializeField] List<BoltEntity> entitiesList;
    [SerializeField] List<int> distanceToEntities;
    int entitiesDamaged;
    bool collided;
    HitDamageUI hitDamageUI;

    string teamTag;
    string enemyTeamTag;

    private void Start()
    {
        StartCoroutine(DestroyFallBack(destroyTime));
    }


    public override void Attached()
    {
        state.SetTransforms(state.ProjectileTransform, transform);
    }

    public void DetermineDamage(bool state)
    {
        if (state && !healBall && !stunBall)
            ballValue *= powerUp;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == playerLayer && healBall && !collided)
        {
            if (collision.collider.CompareTag(teamTag) || collision.collider.GetComponentInParent<Health>().CompareTag(teamTag))
            {
                directHit = true;
                collided = true;
                SendHealing(directHitValue, collision.gameObject.GetComponentInParent<BoltEntity>());
            }
            else return;
        }

        if (collided || directHit)
            return;

        collided = true;
        GetHitObjects();
    }

    void GetHitObjects()
    {
        if (entity.IsOwner)
        {
            Collider[] hitObjects = Physics.OverlapSphere(transform.position, radius);
            //check colliders for tag and bolt component
            foreach (Collider collider in hitObjects)
            {
                string entityTag = collider.tag;
                BoltEntity boltEntity = collider.GetComponentInParent<BoltEntity>();
                Health health = boltEntity.GetComponentInChildren<Health>();

                if (!entitiesList.Contains(boltEntity) && health)
                {
                    entitiesList.Add(boltEntity);

                    if (!stunBall && !healBall)
                        GetDistanceToEntities(collider);
                }
            }

            if (entitiesList.Count == 0)
                DestroyBall();
            else
                SendRequestInfo();
        }
    }

    void GetDistanceToEntities(Collider collider)
    {
        float distance = Vector3.Distance(transform.position, collider.transform.position);
        int distanceRound = Mathf.RoundToInt(distance);
        if(distanceRound == 0) { distanceRound = 1; }
        distanceToEntities.Add(distanceRound);
    }

    void SendRequestInfo()
    {
        int totalDamage = 0;
        int amountOfEnemiesDamaged = 0;

        foreach (BoltEntity entity in entitiesList)
        {
            if (entity.gameObject == playerEntity.gameObject)
            {
                entitiesDamaged++;
            }
            else if (stunBall)
            {
                if (entity.GetComponentInChildren<Health>().CompareTag(enemyTeamTag))
                    SendStun(stunTime, entity);

                entitiesDamaged++;
            }
            else if (!healBall)
            {
                amountOfEnemiesDamaged++;

                if(entity.GetComponentInChildren<Health>().CompareTag(enemyTeamTag))
                    SendDamage(ballValue / distanceToEntities[entitiesDamaged], entity);

                totalDamage += ballValue / distanceToEntities[entitiesDamaged];
                entitiesDamaged++;

                if (amountOfEnemiesDamaged == distanceToEntities.Count)
                {
                    if (totalDamage != 0)
                    {
                        hitDamageUI.SendDamage(0, true, totalDamage);
                    }
                }
            }
            else if (healBall)
            {
                if(entity.GetComponentInChildren<Health>().CompareTag(teamTag))
                    SendHealing(ballValue, entity);

                entitiesDamaged++;
            }
        }

        if(entitiesList.Count == entitiesDamaged)
            DestroyBall();
    }

    void SendStun(float duration, BoltEntity entityShot)
    {
        var request = StunEvent.Create();
        request.Duration = duration;
        request.EntityShot = entityShot;
        request.Send();
    }

    void SendDamage(int damage, BoltEntity entityShot)
    {
        var request = DamageRequest.Create();
        request.EntityShot = entityShot;
        request.Damage = damage;
        request.EntityShooter = playerEntity;
        request.Send();
    }

    void SendHealing(int healing, BoltEntity entityShot)
    {
        var request = HealRequest.Create();
        request.Healing = healing;
        request.EntityShot = entityShot;
        request.HealStim = false;
        request.Send();

        if (directHit)
            DestroyBall();
    }

    void DestroyBall()
    {
        BoltNetwork.Destroy(gameObject);
    }

    public IEnumerator DestroyFallBack(float time)
    {
        yield return new WaitForSeconds(time);

        DestroyBall();

        StopCoroutine(nameof(DestroyFallBack));
    }

    public void SetHitDamageUI(HitDamageUI ui) { hitDamageUI = ui; }
    public void SetPlayerHit(bool heal, bool stuns) { healBall = heal; stunBall = stuns; }

    public void SetTags(string team, string enemyTeam)
    {
        teamTag = team;
        enemyTeamTag = enemyTeam;
    }
}
