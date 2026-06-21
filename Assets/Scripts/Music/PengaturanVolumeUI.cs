using UnityEngine;
using UnityEngine.UI;
public class PengaturanVolumeUI : MonoBehaviour
{
    [Header("Slider Volume")]
    public Slider sliderBGM;
    public Slider sliderSFX;

    private void Start()
    {
        SinkronkanNilai();
    }
    private void OnEnable()
    {
        SinkronkanNilai();
    }

    private void SinkronkanNilai()
    {
        if (AudioManager.instance != null)
        {
            Debug.Log("AudioManager instance found. Setting slider values.");
            if (sliderBGM != null && sliderSFX != null)
            {
                Debug.Log("Setting slider values to AudioManager's master volume.");
                sliderBGM.value = AudioManager.instance.masterBGMVolume;
                sliderSFX.value = AudioManager.instance.masterSFXVolume;
            }
        }
    }
}
