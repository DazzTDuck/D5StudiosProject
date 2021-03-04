using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    static Sound[] staticSounds;

    public bool awakeBGMusic;

    public static AudioManager instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (var s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.outputAudioMixerGroup = s.mixer;

            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;

            s.source.loop = s.loop;

        }

        staticSounds = sounds;
    }

    private void Start()
    {
        //main menu music
        if(awakeBGMusic)
        PlaySound("BackgroundMusic");
    }

    public static void PlaySound(string name)
    {
        Sound s = Array.Find(staticSounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + "not found");
            return;
        }

        s.source.Play();

        if (s.clip == null)
        {
            Debug.LogWarning("No AudioClip Specified for" + name);
            return;
        }
    }
}
