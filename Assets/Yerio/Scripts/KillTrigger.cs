using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var health = other.gameObject.GetComponent<Health>();
        if (health)
        {
            //Create DamageRequest, set entity to ent and Damage to damage, then send
            var request = DamageRequest.Create();
            request.Entity = health.GetComponentInParent<BoltEntity>();
            request.Damage = 9999;
            request.Send();
        }
    }
}
