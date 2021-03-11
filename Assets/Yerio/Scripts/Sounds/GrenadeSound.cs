using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class GrenadeSound : Bolt.EntityBehaviour<IGrenadeSoundState>
{
    [SerializeField] float timeToDestroy;

    public override void Attached()
    {
        if (entity.IsOwner)
        {
            state.SetTransforms(state.ObjectTransform, transform);
            StartSoundRequest();
            StartCoroutine(DestroyObject());
        }
    }

    public void StartSoundRequest() 
    {
        var request = PlayGrenadeSound.Create();
        request.GrenadeSoundEntity = entity;
        request.Send();
    }

    public void PlaySound()
    {
        var source = GetComponent<AudioSource>();
        source.Play();
    }

    IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(timeToDestroy);

        BoltNetwork.Destroy(gameObject);
    }
}
