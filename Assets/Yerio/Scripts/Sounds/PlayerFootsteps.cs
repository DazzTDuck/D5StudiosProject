using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFootsteps : Bolt.EntityBehaviour<PlayerController>
{
    [SerializeField] Timer stepTimer;
    [SerializeField] AudioSource source;
    [SerializeField] float stepDelay;
    [SerializeField] PlayerController player;
    [Space]
    [SerializeField] AudioClip[] footstepSounds;

    bool fistStepSet = false;

    private void Update()
    {
        //step timing
        if (stepTimer.IsTimerComplete() && player.state.IsWalking && entity.IsOwner && !player.isCrouching)
        {
            if(!fistStepSet) { StepSoundRequest(); fistStepSet = true; }
            stepTimer.SetTimer(stepDelay, () => StepSoundRequest());
        }

        if(!player.state.IsWalking && entity.IsOwner && fistStepSet != false) { fistStepSet = false; }
    }
    public void StepSoundRequest()
    {
        if (player.state.IsWalking && entity.IsOwner && !player.isCrouching)
        {
            var request = PlayFootstep.Create();
            request.EntityToPlayAt = entity;
            request.Send();
        }      
    }

    public void PlayStepSound()
    {
        if (player.state.IsWalking)
        {
            var i = Random.Range(0, footstepSounds.Length);
            source.clip = footstepSounds[i];
            source.Play();
        }
            
    }
}
