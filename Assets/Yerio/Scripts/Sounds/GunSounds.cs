using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class GunSounds : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] AudioClip gunFireSound;
    [SerializeField] AudioClip gunReloadSound;
    [SerializeField] AudioSource gunSoundSource;

    public void PlaySound(string sound)
    {
        var request = PlayGunSound.Create();
        request.EntityToPlayAt = entity;
        request.SoundToPlay = sound;
        request.Send();
    }

    public void PlayFireSound()
    {
        gunSoundSource.clip = gunFireSound;
        gunSoundSource.Play();
    }
    public void PlayReloadSound()
    {
        gunSoundSource.clip = gunReloadSound;
        gunSoundSource.Play();
    }
}
