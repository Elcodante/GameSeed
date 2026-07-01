using UnityEngine;

[RequireComponent(typeof(HingeJoint))]
public class MekanikJari : MonoBehaviour
{
    [Header("Pengaturan Jari")]
    public float kecepatanPutar = 1f;
    public string namaBagianTangan = "Jari Tengah";
    public bool balikArahGerak = false;

    [Header("Pengaturan Kekuatan (Otot)")]
    [Tooltip("Semakin besar, semakin keras jari menolak ditekuk oleh meja")]
    public float kekuatanOtot = 5000f; // Default diubah jadi 5000!
    [Tooltip("Mencegah jari bergetar/memantul")]
    public float remOtot = 100f;

    private Vector2 posisiMouseLama;
    private HingeJoint engsel;
    private JointSpring perEngsel;
    private bool sedangDitarik = false;
    private Camera kameraUtama;
    private Vector2 arahTarikanTerkunci;

    void Start()
    {
        engsel = GetComponent<HingeJoint>();
        engsel.useSpring = true;
        perEngsel = engsel.spring;

        // SEKARANG MENGGUNAKAN VARIABEL AGAR BISA DIUBAH DI INSPECTOR
        perEngsel.spring = kekuatanOtot;
        perEngsel.damper = remOtot;
        engsel.spring = perEngsel;

        kameraUtama = Camera.main;
    }

    void Update()
    {
        if (sedangDitarik)
        {
            Vector3 posisiMouseSekarang = Input.mousePosition;
            bool mouseDilepas = !Input.GetMouseButton(0);
            bool mouseKeluarLayar = posisiMouseSekarang.x < 0 || posisiMouseSekarang.y < 0 ||
                                    posisiMouseSekarang.x > Screen.width || posisiMouseSekarang.y > Screen.height;

            if (mouseDilepas || mouseKeluarLayar)
            {
                LepasPegangan();
            }
        }
    }

    void OnMouseEnter()
    {
        if (!sedangDitarik && UIManagerTangan.instance != null)
            UIManagerTangan.instance.MulaiSorot(namaBagianTangan, transform);
    }

    void OnMouseExit()
    {
        if (!sedangDitarik && UIManagerTangan.instance != null)
            UIManagerTangan.instance.BerhentiSorot();
    }

    void OnMouseDown()
    {
        sedangDitarik = true;
        posisiMouseLama = Input.mousePosition;

        // Simulasi Arah Anti-Terbalik
        Collider col = GetComponent<Collider>();
        Vector3 pusatMassaDunia = col != null ? col.bounds.center : transform.position + transform.forward;
        Vector3 pusatLokal = transform.InverseTransformPoint(pusatMassaDunia);
        if (pusatLokal.magnitude < 0.01f) pusatLokal = Vector3.forward;

        Quaternion rotasiSimulasi = Quaternion.AngleAxis(5f, engsel.axis);
        Vector3 pusatBaruLokal = rotasiSimulasi * pusatLokal;

        Vector3 posisiDuniaAwal = transform.TransformPoint(pusatLokal);
        Vector3 posisiDuniaBaru = transform.TransformPoint(pusatBaruLokal);

        Vector2 posisiLayarAwal = kameraUtama.WorldToScreenPoint(posisiDuniaAwal);
        Vector2 posisiLayarBaru = kameraUtama.WorldToScreenPoint(posisiDuniaBaru);

        arahTarikanTerkunci = (posisiLayarBaru - posisiLayarAwal).normalized;
        if (arahTarikanTerkunci == Vector2.zero) arahTarikanTerkunci = Vector2.up;

        if (UIManagerTangan.instance != null)
            UIManagerTangan.instance.SedangDitarik(namaBagianTangan);
    }

    void OnMouseUp()
    {
        LepasPegangan();
    }

    void OnMouseDrag()
    {
        if (!sedangDitarik) return;

        Vector2 bedaGeser = (Vector2)Input.mousePosition - posisiMouseLama;
        float putaran = Vector2.Dot(bedaGeser, arahTarikanTerkunci) * kecepatanPutar;

        if (balikArahGerak) putaran = -putaran;

        perEngsel.targetPosition += putaran;

        if (engsel.useLimits)
        {
            perEngsel.targetPosition = Mathf.Clamp(perEngsel.targetPosition, engsel.limits.min, engsel.limits.max);
        }

        engsel.spring = perEngsel;
        posisiMouseLama = Input.mousePosition;
    }

    private void LepasPegangan()
    {
        if (sedangDitarik)
        {
            sedangDitarik = false;
            if (UIManagerTangan.instance != null)
                UIManagerTangan.instance.BerhentiSorot();
        }
    }
}