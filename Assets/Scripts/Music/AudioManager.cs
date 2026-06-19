using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.PlayerLoop;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; // Singleton instance

    [Header("Daftar Suara")]
    public Sound[] bgmSounds; // Array of background music sounds
    public Sound[] sfxSounds; // Array of sound effects

    [Header("Pengaturan Object Pool SFX")]
    public int sfxPoolSize = 20; // Size of the sound effects object pool
    private List<AudioSource> sfxPool; // Object pool for sound effects

    private AudioSource bgmSource; // AudioSource for background music

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep the AudioManager across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate AudioManager
            return;
        }

        bgmSource = gameObject.AddComponent<AudioSource>(); // Add AudioSource for BGM
        bgmSource.spatialBlend = 0; // Set to 2D sound

        InitializeSFXPool(); // Initialize the SFX object pool
    }

    private void InitializeSFXPool()
    {
        sfxPool = new List<AudioSource>();

        GameObject poolContainer = new GameObject("SFXPool"); // Container for SFX AudioSources
        poolContainer.transform.SetParent(this.transform);

        for(int i = 0; i < sfxPoolSize; i++)
        {
            GameObject sfxObject = new GameObject("SFX_Source_" + i);
            sfxObject.transform.SetParent(poolContainer.transform);

            AudioSource source = sfxObject.AddComponent<AudioSource>();

            source.spatialBlend = 1; // Set to 3D sound
            source.rolloffMode = AudioRolloffMode.Linear; // Linear rolloff for 3D sound
            source.maxDistance = 20f; // Max distance for sound to be heard
            source.minDistance = 1f; // Min distance for sound to be at full volume

            source.playOnAwake = false; // Don't play on awake

            sfxPool.Add(source); // Add to the pool
        }
    }

    public void PlaySFX3D(string soundName, Vector3 position)
    {
        Sound s = Array.Find(sfxSounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("SFX tidak ditemukan: " + soundName);
            return;
        }

        AudioSource availableSource = GetAvailableSFXSource();

        if (availableSource != null)
        {
            availableSource.transform.position = position; // Set the position for 3D sound

            availableSource.clip = s.clip;
            availableSource.volume = s.volume;
            availableSource.pitch = s.pitch;

            availableSource.Play();
        }
        else
        {
            Debug.LogWarning("Tidak ada AudioSource yang tersedia di pool untuk SFX: " + soundName);
        }
    }

    private AudioSource GetAvailableSFXSource()
    {
        for(int i = 0; i < sfxPool.Count; i++)
        {
            if (!sfxPool[i].isPlaying)
            {
                return sfxPool[i]; // Return the first available AudioSource
            }
        }
        return null;
    }
}
