using UnityEngine;

[RequireComponent(typeof(HingeJoint))]
public class MekanikJari : MonoBehaviour
{
    // Kecepatan putar bisa dibesarkan karena kita mengubah sudut target engsel
    public float kecepatanPutar = 150f;
    public string namaBagianTangan = "Jari";

    private Vector3 posisiMouseLama;
    private HingeJoint engsel;
    private JointSpring perEngsel;
    private bool sedangDitarik = false;

    void Start()
    {
        // Mengambil komponen HingeJoint di jari ini
        engsel = GetComponent<HingeJoint>();

        // Menyalakan fitur otot/pegas pada engsel
        engsel.useSpring = true;
        perEngsel = engsel.spring;

        // Memberikan kekuatan pada engsel agar kuat menahan posisinya
        // (Pastikan angka spring ini sesuai dengan kekuatan kuli yang udah berhasil tadi ya!)
        perEngsel.spring = 1000f;
        perEngsel.damper = 50f;
        engsel.spring = perEngsel;
    }

    void OnMouseEnter()
    {
        if (!sedangDitarik && UIManagerTangan.instance != null)
        {
            UIManagerTangan.instance.TampilkanUI(namaBagianTangan);
        }
    }

    void OnMouseExit()
    {
        if (!sedangDitarik && UIManagerTangan.instance != null)
        {
            UIManagerTangan.instance.SembunyikanUI();
        }
    }

    void OnMouseDown()
    {
        sedangDitarik = true;
        posisiMouseLama = Input.mousePosition;

        if (UIManagerTangan.instance != null)
        {
            UIManagerTangan.instance.SedangDitarik(namaBagianTangan);
        }
    }

    void OnMouseUp()
    {
        LepasPegangan(); // Kita panggil fungsi khusus di bawah biar rapi
    }

    void OnMouseDrag()
    {
        // 1. Kalau status sedangDitarik = false (berarti udah lepas pegangan), hentikan proses!
        if (!sedangDitarik) return;

        // 2. DETEKTOR BATAS LAYAR!
        // Jika kursor keluar dari tepi kiri (0), kanan (Screen.width), bawah (0), atau atas (Screen.height)
        if (Input.mousePosition.x < 0 || Input.mousePosition.x > Screen.width ||
            Input.mousePosition.y < 0 || Input.mousePosition.y > Screen.height)
        {
            LepasPegangan(); // Paksa lepas pegangan!
            return; // Hentikan kode putaran di bawahnya
        }

        // --- Logika perputaran jarimu yang sudah benar ---
        Vector3 bedaGeser = Input.mousePosition - posisiMouseLama;

        // Hitung seberapa jauh sudut harus berubah
        float putaran = -bedaGeser.y * kecepatanPutar * Time.deltaTime;

        // Ubah target posisi sudut engsel, bukan transform-nya!
        perEngsel.targetPosition += putaran;

        // Terapkan kembali ke engsel
        engsel.spring = perEngsel;

        posisiMouseLama = Input.mousePosition;
    }

    private void LepasPegangan()
    {
        if (sedangDitarik)
        {
            sedangDitarik = false;
            if (UIManagerTangan.instance != null)
            {
                UIManagerTangan.instance.SembunyikanUI();
            }
        }
    }
}