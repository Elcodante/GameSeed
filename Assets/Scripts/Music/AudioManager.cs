using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Pengaturan Volume Master UI")]
    [Range(0f, 1f)] public float masterBGMVolume = 1f;
    [Range(0f, 1f)] public float masterSFXVolume = 1f;

    void Awake()
    {
        if (instance == null)
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

        for (int i = 0; i < sfxPoolSize; i++)
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

    // ==========================================
    // FUNGSI PENGATURAN SLIDER UI
    // ==========================================
    public void SetBGMVolume(float volume)
    {
        masterBGMVolume = volume;
        if (bgmSource != null)
        {
            bgmSource.volume = masterBGMVolume; // Update volume BGM secara real-time
            Debug.Log("BGM volume updated to: " + masterBGMVolume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        masterSFXVolume = volume;
        Debug.Log("SFX volume updtaed to: " + masterSFXVolume);
    }

    // ==========================================
    // FUNGSI MEMUTAR SFX
    // ==========================================
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

            // Kalikan volume asli SFX dengan masterSFXVolume dari Slider UI
            availableSource.volume = s.volume * masterSFXVolume;
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
        for (int i = 0; i < sfxPool.Count; i++)
        {
            if (!sfxPool[i].isPlaying)
            {
                return sfxPool[i]; // Return the first available AudioSource
            }
        }
        return null;
    }

    // ==========================================
    // FUNGSI MEMUTAR & TRANSISI BGM
    // ==========================================
    public void PlayBGM(string soundName)
    {
        Sound s = Array.Find(bgmSounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("BGM tidak ditemukan: " + soundName);
            return;
        }

        // Jika BGM sudah berputar, lakukan transisi menyilang (Crossfade)
        if (bgmSource.isPlaying)
        {
            StartCoroutine(CrossfadeBGM(s));
        }
        else
        {
            // Jika belum ada musik, mainkan langsung
            bgmSource.clip = s.clip;
            bgmSource.volume = s.volume * masterBGMVolume;
            bgmSource.pitch = s.pitch;
            bgmSource.loop = s.loop; // Pastikan kamu mencentang loop di Inspector untuk lagu ini
            bgmSource.Play();
        }
    }

    // Sistem untuk memudarkan lagu lama dan memunculkan lagu baru secara perlahan
    private IEnumerator CrossfadeBGM(Sound newBGM)
    {
        float fadeTime = 1.0f; // Waktu transisi 1 detik
        float startVolume = bgmSource.volume;

        // Tahap 1: Fade Out (Redupkan lagu lama)
        while (bgmSource.volume > 0)
        {
            bgmSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        // Tahap 2: Ganti Kaset
        bgmSource.clip = newBGM.clip;
        bgmSource.pitch = newBGM.pitch;
        bgmSource.loop = newBGM.loop;
        bgmSource.Play();

        // Tahap 3: Fade In (Keraskan lagu baru perlahan sesuai pengaturan Slider)
        float targetVolume = newBGM.volume * masterBGMVolume;
        while (bgmSource.volume < targetVolume)
        {
            bgmSource.volume += targetVolume * Time.deltaTime / fadeTime;
            yield return null;
        }
        bgmSource.volume = targetVolume; // Pastikan angkanya akurat di akhir
    }
}