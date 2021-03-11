using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

[Serializable]
public class PlayerSounds
{
    public string name;
    public AudioClip clip;
    public float volume;
}

public class PlayPlayerSounds : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] PlayerSounds[] playerSounds;
    [SerializeField] AudioSource source;

    public void PlaySoundRequest(int soundIndex)
    {
        var request = PlayPlayerSoundRequest.Create();
        request.EntityToPlayAt = entity;
        request.SoundToPlayIndex = soundIndex;
        request.Send();
    }

    public void PlayPlayerSound(int index)
    {             
        source.clip = playerSounds[index].clip;
        source.volume = playerSounds[index].volume;
        source.Play();
    }
}
