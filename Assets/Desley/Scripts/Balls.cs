using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balls : Bolt.EntityBehaviour<IProjectileState>
{
    [SerializeField] int playerLayer;
    bool stunsEnemies;
    bool directHit;
    bool healBall;

    [Space, SerializeField] int ballValue;
    [SerializeField] int directHitValue;
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
        base.Attached();
        state.SetTransforms(state.ProjectileTransform, transform);
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
                BoltEntity boltEntity = collider.GetComponent<BoltEntity>();
                if (!boltEntity) { boltEntity = collider.GetComponentInParent<BoltEntity>(); }

                if (!entitiesList.Contains(boltEntity))
                {
                    if (entityTag == "Enemy" || entityTag == teamTag || entityTag == enemyTeamTag)
                    {
                        entitiesList.Add(boltEntity);
                        if (!stunsEnemies && !healBall)
                            GetDistanceToEntities(collider);
                    }
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
            if (stunsEnemies)
            {
                entity.GetComponent<EnemyMove>().StunEnemy(stunTime);
                entitiesDamaged++;
            }
            else if (!healBall)
            {
                amountOfEnemiesDamaged++;
                SendDamage(ballValue / distanceToEntities[entitiesDamaged], true, entity);
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
                SendHealing(ballValue, entity);
                entitiesDamaged++;
            }
        }

        if(entitiesList.Count == entitiesDamaged)
            DestroyBall();
    }

    void SendDamage(int damage, bool isEnemy, BoltEntity entityShot)
    {
        var request = DamageRequest.Create();
        request.EntityShot = entityShot;
        request.Damage = damage;
        request.IsEnemy = isEnemy;
        request.EntityShooter = playerEntity;
        request.Send();
    }

    void SendHealing(int healing, BoltEntity entityShot)
    {
        var request = HealRequest.Create();
        request.Healing = healing;
        request.EntityShot = entityShot;
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
    public void SetPlayerHit(bool heal, bool stuns) { healBall = heal; stunsEnemies = stuns; }

    public void SetTags(string team, string enemyTeam)
    {
        teamTag = team;
        enemyTeamTag = enemyTeam;
    }
}
