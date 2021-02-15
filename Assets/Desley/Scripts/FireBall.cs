using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : Bolt.EntityBehaviour<IFireBallState>
{  
    [SerializeField] LayerMask playerLayer;
    public bool canHitPlayer;

    [Space, SerializeField] int damage;
    [SerializeField] float radius;

    [Space, SerializeField] float destroyTime;

    [Space] public BoltEntity playerEntity;
    [SerializeField] List<BoltEntity> enemyEntities;
    [SerializeField] List<BoltEntity> playerEntities;
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
        if (collided || collision.gameObject.layer == playerLayer && !canHitPlayer)
            return;

        collided = true;
        GetHitObjects();
    }

    void GetHitObjects()
    {
        Collider[] hitObjects = Physics.OverlapSphere(transform.position, radius);
        //check which colliders have health scripts
        foreach(Collider collider in hitObjects)
        {
            string entityTag = collider.tag;
            BoltEntity boltEntity = collider.GetComponent<BoltEntity>();

            if (entityTag == "Enemy" && !enemyEntities.Contains(boltEntity) && entity.IsOwner)
            {
                enemyEntities.Add(boltEntity);
                GetDistanceToEntities(collider);
            }
            else if (entityTag == "Player" && !playerEntities.Contains(boltEntity) && canHitPlayer && entity.IsOwner)
            {
                playerEntities.Add(boltEntity);
                GetDistanceToEntities(collider);
            }
        }

        if (enemyEntities.Count == 0 && playerEntities.Count == 0)
            DestroyFireBall();
        else
            SendDamageInfo();
    }

    void GetDistanceToEntities(Collider collider)
    {
        float distance = Vector3.Distance(transform.position, collider.transform.position);
        int distanceRound = Mathf.RoundToInt(distance);
        if(distanceRound == 0) { distanceRound = 1; }
        distanceToEntities.Add(distanceRound);
    }

    void SendDamageInfo()
    {
        int totalDamage = 0;
        int amountOfEnemiesDamaged = 0;

        //send damage to all enemies
        foreach (BoltEntity entity in enemyEntities)
        {
            amountOfEnemiesDamaged++;
            SendDamage(damage / distanceToEntities[entitiesDamaged], true, entity);
            totalDamage += damage / distanceToEntities[entitiesDamaged];
            entitiesDamaged++;

            if (amountOfEnemiesDamaged == distanceToEntities.Count)
            {
                if (totalDamage != 0)
                {
                    hitDamageUI.SendDamage(0, true, totalDamage);
                }
            }
        }
        //send damage to all players
        foreach (BoltEntity entity in playerEntities)
        {
            amountOfEnemiesDamaged++;
            SendDamage(damage / distanceToEntities[entitiesDamaged], false, entity);
            totalDamage += damage / distanceToEntities[entitiesDamaged];
            entitiesDamaged++;

            if (amountOfEnemiesDamaged == distanceToEntities.Count)
            {
                if (totalDamage != 0)
                {
                    hitDamageUI.SendDamage(0, true, totalDamage);
                }
            }
        }

        if (enemyEntities.Count + playerEntities.Count == entitiesDamaged)
            DestroyFireBall();
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

    void DestroyFireBall()
    {
        BoltNetwork.Destroy(gameObject);
    }

    public IEnumerator DestroyFallBack(float time)
    {
        yield return new WaitForSeconds(time);

        BoltNetwork.Destroy(gameObject);

        StopCoroutine(nameof(DestroyFallBack));
    }

    public void SetHitDamageUI(HitDamageUI ui) { hitDamageUI = ui; }
}
