using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // Wajib ditambahkan agar script mengenali TextMeshPro

public class ButtonPop : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Pengaturan Teks (TextMeshPro)")]
    [Tooltip("Kosongkan saja, script akan otomatis mencari teks di dalam tombol")]
    public TextMeshProUGUI buttonText;
    [Tooltip("Warna teks saat mouse disorot (Default: Putih terang)")]
    public Color hoverColor = Color.white;
    public float colorSpeed = 15f; // Kecepatan transisi warna

    private Color originalColor;
    private Color targetColor;

    [Header("Pengaturan Animasi Skala")]
    public float hoverMultiplier = 1.1f;
    public float smoothTime = 0.05f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        // 1. Persiapan Animasi Skala
        originalScale = transform.localScale;
        targetScale = originalScale;

        // 2. Persiapan Animasi Warna Teks
        // Jika kolom buttonText di Inspector kosong, otomatis cari komponen TextMeshPro di objek anak
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
        }

        // Jika teks berhasil ditemukan, simpan warna aslinya
        if (buttonText != null)
        {
            originalColor = buttonText.color;
            targetColor = originalColor;
        }
    }

    void Update()
    {
        // Update Animasi Skala (SmoothDamp yang anti-lompat)
        transform.localScale = Vector3.SmoothDamp(
            transform.localScale,
            targetScale,
            ref velocity,
            smoothTime
        );

        // Update Animasi Warna Teks (Lerp aman digunakan untuk warna karena tidak memicu lag UI)
        if (buttonText != null)
        {
            buttonText.color = Color.Lerp(buttonText.color, targetColor, Time.deltaTime * colorSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Ubah target skala dan target warna saat disorot
        targetScale = originalScale * hoverMultiplier;
        targetColor = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Kembalikan target skala dan target warna ke semula
        targetScale = originalScale;
        targetColor = originalColor;
    }
}