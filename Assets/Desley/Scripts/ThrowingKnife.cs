using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingKnife : Bolt.EntityBehaviour<IProjectileState>
{
    [SerializeField] int damage;
    [SerializeField] int hsMultiplier;

    bool collided;

    string teamTag;
    string enemyTeamTag;

    public override void Attached()
    {
        base.Attached();
        state.SetTransforms(state.ProjectileTransform, transform);
    }

    private void Start()
    {
        StartCoroutine(DestroyFallBack(10));
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if (collision.collider.CompareTag(teamTag) || collision.collider.GetComponentInParent<Health>().CompareTag(teamTag))
        //    return;

        if (!collided)
        {
            CheckCollision(collision.gameObject);
            collided = true;
        }
    }

    void CheckCollision(GameObject objectHit)
    {
        BoltEntity boltEntity = objectHit.GetComponent<BoltEntity>();
        if (!boltEntity) { boltEntity = objectHit.GetComponentInParent<BoltEntity>(); }

        if(boltEntity)
        {
            if (objectHit.CompareTag("Enemy"))
            {
                SendDamage(damage, true, boltEntity);
            }
            else if (objectHit.CompareTag("EnemyHead"))
            {
                SendDamage(damage * hsMultiplier, true, boltEntity);
            }
            else if (objectHit.CompareTag(enemyTeamTag))
            {
                SendDamage(damage, false, boltEntity);
            }
            else if (objectHit.GetComponentInParent<Health>().CompareTag(enemyTeamTag))
            {
                SendDamage(damage * hsMultiplier, false, boltEntity);
            }
        }
        else
            DestroyKnife();
    }

    void SendDamage(int damage, bool isEnemy, BoltEntity entityShot)
    {
        var request = DamageRequest.Create();
        request.EntityShot = entityShot;
        request.Damage = damage;
        request.IsEnemy = isEnemy;
        request.EntityShooter = entity;
        request.Send();

        DestroyKnife();
    }

    void DestroyKnife()
    {
        BoltNetwork.Destroy(gameObject);
    }

    public IEnumerator DestroyFallBack(float time)
    {
        yield return new WaitForSeconds(time);

        DestroyKnife();
    }

    public void SetTags(string team, string enemyTag)
    {
        teamTag = team;
        enemyTeamTag = enemyTag;
    }
}
