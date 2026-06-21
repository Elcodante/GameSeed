using UnityEngine;
using UnityEngine.UI;

public class PanoramaPingPong : MonoBehaviour
{
    private RawImage panoramaImage;

    [Header("Pengaturan Ping-Pong")]
    [Tooltip("Kecepatan gambar bergerak")]
    public float speed = 0.05f;

    [Tooltip("Batas maksimal pergeseran. Rumus ideal: 1.0 - nilai W pada UV Rect")]
    public float maxPanDistance = 0.3f;

    void Start()
    {
        panoramaImage = GetComponent<RawImage>();
    }

    void Update()
    {
        Rect currentRect = panoramaImage.uvRect;

        // Mathf.PingPong otomatis membuat angka naik perlahan ke maxPanDistance, 
        // lalu turun kembali ke 0 secara berulang-ulang tanpa henti.
        currentRect.x = Mathf.PingPong(Time.time * speed, maxPanDistance);

        panoramaImage.uvRect = currentRect;
    }
}