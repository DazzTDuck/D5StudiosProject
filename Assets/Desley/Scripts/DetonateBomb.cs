using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetonateBomb : Bolt.EntityBehaviour<IProjectileState>
{
    [SerializeField] GameObject clusterFragBomb;
    [SerializeField] Vector3[] directions; 
    [SerializeField] float explosionForce;
    [SerializeField] float radius;
    [SerializeField] float upforce;
    [SerializeField] float fallbackTime;

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
        if (!collision.collider.CompareTag(teamTag))
            Detonate();
    }

    void Detonate()
    {
        foreach(Vector3 dir in directions)
        {
            BoltNetwork.Instantiate(clusterFragBomb, transform.position + dir, Quaternion.identity);
        }

        Collider[] hitObjects = Physics.OverlapSphere(transform.position, radius);
        foreach(Collider hit in hitObjects)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.AddExplosionForce(explosionForce, transform.position, radius, upforce, ForceMode.Impulse);
            }
        }

        DestroyObject();
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
