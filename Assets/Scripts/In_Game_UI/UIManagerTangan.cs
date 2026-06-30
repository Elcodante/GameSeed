using UnityEngine;
using TMPro; // Wajib untuk TextMeshPro

public class UIManagerTangan : MonoBehaviour
{
    public static UIManagerTangan instance;

    [Header("UI Elemen")]
    public GameObject panelInteraksi;
    public TextMeshProUGUI teksInteraksi;
    public GameObject lingkaranCursor;

    // VARIABEL BARU: Menyimpan tulang mana yang sedang disentuh
    private Transform targetTulang;
    private Camera kameraUtama;

    void Awake()
    {
        instance = this;
        panelInteraksi.SetActive(false);
        lingkaranCursor.SetActive(false);
        kameraUtama = Camera.main; // Simpan data kamera utama
    }

    void Update()
    {
        // Jika lingkaran aktif dan ada tulang yang ditarget...
        if (lingkaranCursor.activeSelf && targetTulang != null)
        {
            // Sulap posisi 3D tulang menjadi posisi 2D di layar monitor, 
            // lalu tempelkan lingkarannya di sana!
            lingkaranCursor.transform.position = kameraUtama.WorldToScreenPoint(targetTulang.position);
        }
    }

    // FUNGSI DIUBAH: Sekarang meminta "Transform" dari tulang yang disentuh
    public void MulaiSorot(string namaBagian, Transform posisiTulang)
    {
        targetTulang = posisiTulang; // Kunci target tulangnya
        teksInteraksi.text = "[Tahan] " + namaBagian;
        panelInteraksi.SetActive(true);
        lingkaranCursor.SetActive(true);
    }

    public void BerhentiSorot()
    {
        targetTulang = null; // Kosongkan target
        panelInteraksi.SetActive(false);
        lingkaranCursor.SetActive(false);
    }

    public void SedangDitarik(string namaBagian)
    {
        teksInteraksi.text = "Menarik: " + namaBagian;
    }
}