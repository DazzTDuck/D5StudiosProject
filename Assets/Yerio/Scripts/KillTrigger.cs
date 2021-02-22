using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTrigger : MonoBehaviour
{
    bool requestSent = false;

    private void OnTriggerEnter(Collider other)
    {
        var health = other.gameObject.GetComponent<Health>();
        if (health && !requestSent)
        {
            health.killTrigger = true;

            //Create DamageRequest, set entity to ent and Damage to damage, then send
            var request = DamageRequest.Create();
            request.EntityShot = health.GetComponentInParent<BoltEntity>();
            request.Damage = 100;
            request.Send();
            requestSent = true;
            StartCoroutine(ResetRequest());
        }
    }

    IEnumerator ResetRequest()
    {
        yield return new WaitForSeconds(1f);

        requestSent = false;
    }
}
