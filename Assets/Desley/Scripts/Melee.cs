using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] Camera cam;
    [SerializeField] Animator animator;
    [SerializeField] HitDamageUI hitDamageUI;
    [SerializeField] Transform shankPoint;
    [SerializeField] int damage;
    [SerializeField] float shankRate, range, damageTime;
    float nextTimeToShank;

    Collider[] hitObjects;
    List<GameObject> hitEnemies = new List<GameObject>();

    bool isShooting;

    public void Update()
    {
        isShooting = Input.GetButton("Fire1");

        if (isShooting && entity.IsOwner && Time.time >= nextTimeToShank && !state.IsDead)
        {
            state.Animator.ResetTrigger("Bonk");
            state.Animator.SetTrigger("Bonk");
            StartCoroutine(WaitForAnimation(damageTime));
            nextTimeToShank = Time.time + 1f / shankRate;
        }
    }

    public override void Attached()
    {
        state.SetAnimator(animator);

        if (entity.IsOwner)
        {
            int index = 9;
            gameObject.layer = index;
            var transforms = GetComponentsInChildren<Transform>();

            foreach (var gameObject in transforms)
            {
                gameObject.gameObject.layer = index;
            }
        }
    }

    public void Shank()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range))
        {
            string entityTag = hit.collider.tag;
            BoltEntity boltEntity = hit.collider.GetComponent<BoltEntity>();
            if(!boltEntity) { boltEntity = hit.collider.GetComponentInParent<BoltEntity>(); }

            if(entityTag == "Enemy" && entity.IsOwner || entityTag == "EnemyHead" && entity.IsOwner)
            {
                SendDamage(damage, true, boltEntity);
            }
            else if(entityTag == "Player" && entity.IsOwner || entityTag == "PlayerHead" && entity.IsOwner)
            {
                SendDamage(damage, false, boltEntity);
            }
        }
    }

    void SendDamage(int damage, bool isEnemy, BoltEntity entityShot)
    {
        var request = DamageRequest.Create();
        request.EntityShot = entityShot;
        request.Damage = damage;
        request.IsEnemy = isEnemy;
        request.EntityShooter = entity;
        request.Send();

        hitDamageUI.SendDamage(damage, true);
    }

    public IEnumerator WaitForAnimation(float time)
    {
        yield return new WaitForSeconds(time);

        Shank();

        StopCoroutine(nameof(WaitForAnimation));
    }
}
