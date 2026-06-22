using UnityEngine;
using TMPro; // Wajib untuk TextMeshPro

public class UIManagerTangan : MonoBehaviour
{
    public static UIManagerTangan instance;

    [Header("UI Elemen")]
    public GameObject panelInteraksi; // Masukkan Teks_Interaksi ke sini
    public TextMeshProUGUI teksInteraksi;
    public GameObject lingkaranCursor;

    void Awake()
    {
        instance = this;
        panelInteraksi.SetActive(false); // Sembunyikan saat game mulai
        lingkaranCursor.SetActive(false); 
    }

    void Update()
    {
        // Jika UI sedang aktif, buat UI tersebut mengikuti posisi kursor mouse
        if (lingkaranCursor.activeSelf)
        {
            lingkaranCursor.transform.position = Input.mousePosition;
        }
    }

    // Fungsi untuk memunculkan UI
    public void TampilkanUI(string namaBagian)
    {
        teksInteraksi.text = "[Tahan] " + namaBagian;
        panelInteraksi.SetActive(true);
        lingkaranCursor.SetActive(true);
    }

    // Fungsi untuk menyembunyikan UI
    public void SembunyikanUI()
    {
        panelInteraksi.SetActive(false);
        lingkaranCursor.SetActive(false);
    }

    public void SedangDitarik(string namaBagian)
    {
        teksInteraksi.text = "Menarik: " + namaBagian;
    }
}