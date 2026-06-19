using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name; //Nama panggilan suara
    public AudioClip clip; // file audio

    [Range(0f, 1f)]
    public float volume = 0.1f; // volume individual suara

    [Range(0.1f, 3f)]
    public float pitch = 1f; // pitch individual suara

    public bool loop = false; // apakah suara diulang
}
