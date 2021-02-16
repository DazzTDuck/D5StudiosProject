using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balls : Bolt.EntityBehaviour<IFireBallState>
{  
    [SerializeField] LayerMask playerLayer;
    public bool canHitPlayer;
    bool directHit;

    [Space, SerializeField] int ballValue;
    [SerializeField] int directHitValue;
    [SerializeField] float radius;

    [Space, SerializeField] float destroyTime;

    [Space] public BoltEntity playerEntity;
    [SerializeField] List<BoltEntity> entitiesList;
    [SerializeField] List<int> distanceToEntities;
    int entitiesDamaged;
    bool collided;
    HitDamageUI hitDamageUI;


    private void Start()
    {
        StartCoroutine(DestroyFallBack(destroyTime));
    }

    public override void Attached()
    {
        base.Attached();
        state.SetTransforms(state.FireBallTransform, transform);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == playerLayer && canHitPlayer)
        {
            SendHealing(directHitValue, collision.gameObject.GetComponentInParent<BoltEntity>());
            directHit = true;
            collided = true;
            return;
        }

        if (collided && !directHit)
            return;

        collided = true;
        GetHitObjects();
    }

    void GetHitObjects()
    {
        if (entity.IsOwner)
        {
            Collider[] hitObjects = Physics.OverlapSphere(transform.position, radius);
            //check which colliders have health scripts
            foreach (Collider collider in hitObjects)
            {
                string entityTag = collider.tag;
                BoltEntity boltEntity = collider.GetComponent<BoltEntity>();
                if (!boltEntity) { boltEntity = collider.GetComponentInParent<BoltEntity>(); }

                if (entityTag == "Enemy" && !entitiesList.Contains(boltEntity) && !canHitPlayer && entity.IsOwner)
                {
                    entitiesList.Add(boltEntity);
                    GetDistanceToEntities(collider);
                }
                else if (entityTag == "Player" && !entitiesList.Contains(boltEntity) && canHitPlayer && entity.IsOwner)
                {
                    entitiesList.Add(boltEntity);
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
            if (!canHitPlayer)
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
            else
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

        Debug.LogWarning(healing);

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
    public void SetPlayerHit(bool playerHit, bool healBall) 
    { 
        canHitPlayer = playerHit; 
    }
}
