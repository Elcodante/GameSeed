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
        if(!sedangDitarik && UIManagerTangan.instance != null)
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
        sedangDitarik = false;

        if(UIManagerTangan.instance != null)
        {
            UIManagerTangan.instance.SembunyikanUI();
        }
    }
    void OnMouseDrag()
    {
        Vector3 bedaGeser = Input.mousePosition - posisiMouseLama;

        // Hitung seberapa jauh sudut harus berubah
        float putaran = -bedaGeser.y * kecepatanPutar * Time.deltaTime;

        // Ubah target posisi sudut engsel, bukan transform-nya!
        perEngsel.targetPosition += putaran;

        // (Opsional) Jika kamu sudah mengatur Limits di HingeJoint agar jari tidak bisa 
        // melengkung ke belakang, hapus tanda komentar pada baris di bawah ini:
        // perEngsel.targetPosition = Mathf.Clamp(perEngsel.targetPosition, engsel.limits.min, engsel.limits.max);

        // Terapkan kembali ke engsel
        engsel.spring = perEngsel;

        posisiMouseLama = Input.mousePosition;
    }
}