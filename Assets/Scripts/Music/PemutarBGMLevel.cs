using UnityEngine;

public class PemutarBGMLevel : MonoBehaviour
{
    [Header("Nama BGM")]
    public string namaBGM;// Nama BGM yang dipakai

    void Start()
    {
        if(AudioManager.instance != null)
        {
            AudioManager.instance.PlayBGM(namaBGM);
        }
        else
        {
            Debug.LogWarning("BGM gagal diputar: Tidak ada AudioManager di Scene");
        }
    }
}
