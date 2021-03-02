using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTrigger : MonoBehaviour
{
    bool requestSent = false;

    private void OnTriggerEnter(Collider other)
    {
        //var health = other.gameObject.GetComponent<Health>();
        //if (health && !requestSent)
        //{
        //    //Create DamageRequest, set entity to ent and Damage to damage, then send
        //    var request = DamageRequest.Create();
        //    request.EntityShot = health.GetComponentInParent<BoltEntity>();
        //    request.Damage = 200;
        //    request.IsEnemy = false;
        //    request.Send();
        //    requestSent = true;
        //    StartCoroutine(ResetRequest());
        //}

        var entity = other.gameObject.GetComponentInParent<BoltEntity>();
        if(entity && !requestSent)
        {
            //Create DestroyRequest, set entity to ent and then send
            var request = DestroyRequest.Create();
            request.Entity = entity;
            request.KillTrigger = true;
            request.IsPlayer = true;
            request.Send();
            requestSent = true;
            StartCoroutine(ResetRequest());
        }
    }
        

    IEnumerator ResetRequest()
    {
        yield return new WaitForSeconds(0.25f);

        requestSent = false;
    }
}
