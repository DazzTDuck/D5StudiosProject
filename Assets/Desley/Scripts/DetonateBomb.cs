using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetonateBomb : Bolt.EntityBehaviour<IProjectileState>
{
    [SerializeField] GameObject clusterFragBomb;
    [SerializeField] GameObject explosionEffect;
    [SerializeField] Vector3[] directions; 
    [SerializeField] float explosionForce;
    [SerializeField] float radius;
    [SerializeField] float upforce;
    [SerializeField] float effectDestroyTime;
    [SerializeField] float fallbackTime;

    bool collided;

    string teamTag;
    string enemyTeamTag;

    public void SetTags(string team, string enemyTeam)
    {
        teamTag = team;
        enemyTeamTag = enemyTeam;
    }

    private void Start()
    {
        StartCoroutine(DestroyObjectFallback(fallbackTime));
    }

    public override void Attached()
    {
        state.SetTransforms(state.ProjectileTransform, transform);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enemyTeamTag != null)
        {
            if (!collision.collider.CompareTag(teamTag) && !collided)
            {
                collided = true;
                Detonate();
            }
        }
    }

    void Detonate()
    {
        if (entity.IsOwner)
        {
            foreach (Vector3 dir in directions)
            {
                var frag = BoltNetwork.Instantiate(clusterFragBomb, transform.position + dir, Quaternion.identity);
                frag.GetComponent<BombFragment>().SetTags(teamTag, enemyTeamTag);
            }

            Collider[] hitObjects = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider hit in hitObjects)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.AddExplosionForce(explosionForce, transform.position, radius, upforce, ForceMode.Impulse);
                }
            }

            var effect = BoltNetwork.Instantiate(explosionEffect, transform.position, Quaternion.identity);
            StartCoroutine(DestroyEffect(effect, effectDestroyTime));
            GetComponent<MeshRenderer>().enabled = false;
        }
    }

    IEnumerator DestroyEffect(BoltEntity effect, float time)
    {
        yield return new WaitForSeconds(time);

        BoltNetwork.Destroy(effect);
        DestroyObject();

        StopCoroutine(nameof(DestroyEffect));
    }

    void DestroyObject()
    {
        BoltNetwork.Destroy(gameObject);
    }

    IEnumerator DestroyObjectFallback(float time)
    {
        yield return new WaitForSeconds(time);

        DestroyObject();

        StopCoroutine(nameof(DestroyObjectFallback));
    }
}
