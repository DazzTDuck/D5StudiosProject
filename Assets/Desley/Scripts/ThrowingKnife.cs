using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingKnife : Bolt.EntityBehaviour<IProjectileState>
{
    [SerializeField] int damage;

    bool collided;

    string teamTag;
    string enemyTeamTag;

    public override void Attached()
    {
        state.SetTransforms(state.ProjectileTransform, transform);
    }

    private void Start()
    {
        StartCoroutine(DestroyFallBack(10));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 15)
            DestroyKnife();

        if (!collided && enemyTeamTag != null)
        {
            CheckCollision(collision.gameObject);
            collided = true;
        }
    }

    void CheckCollision(GameObject objectHit)
    {
        BoltEntity boltEntity = objectHit.GetComponentInParent<BoltEntity>();

        if(boltEntity)
        {
            Health health = boltEntity.GetComponentInChildren<Health>();

            if (health)
            {
                if (health.CompareTag(enemyTeamTag))
                {
                    SendDamage(damage, boltEntity);
                }
            }
        }
        else
            DestroyKnife();
    }

    void SendDamage(int damage, BoltEntity entityShot)
    {
        var request = DamageRequest.Create();
        request.EntityShot = entityShot;
        request.Damage = damage;
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
