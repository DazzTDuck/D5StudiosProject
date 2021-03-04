using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class GunSounds : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] AudioClip gunFireSound;
    [SerializeField] float fireVolumeAmount;
    [SerializeField] AudioClip gunReloadSound;
    [SerializeField] float reloadVolumeAmount;
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
        gunSoundSource.volume = fireVolumeAmount;
        gunSoundSource.Play();
    }
    public void PlayReloadSound()
    {
        gunSoundSource.clip = gunReloadSound;
        gunSoundSource.volume = reloadVolumeAmount;
        gunSoundSource.Play();
    }
}
