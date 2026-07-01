using UnityEngine;

[RequireComponent(typeof(HingeJoint))]
public class MekanikJari : MonoBehaviour
{
    [Header("Pengaturan Jari")]
    public float kecepatanPutar = 1f;
    public string namaBagianTangan = "Jari Tengah";
    public bool balikArahGerak = false;

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

        perEngsel.spring = 1000f;
        perEngsel.damper = 50f;
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

        Vector3 sumbuDunia = transform.TransformDirection(engsel.axis);
        Vector3 posisiLayar = kameraUtama.WorldToScreenPoint(transform.position);
        Vector3 ujungSumbuLayar = kameraUtama.WorldToScreenPoint(transform.position + sumbuDunia);

        Vector2 arahSumbuLayar = new Vector2(ujungSumbuLayar.x - posisiLayar.x, ujungSumbuLayar.y - posisiLayar.y).normalized;
        arahTarikanTerkunci = new Vector2(-arahSumbuLayar.y, arahSumbuLayar.x);

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

        // Nah, bagian ini yang akan membaca settingan "Limits" dari Hinge Joint-mu secara otomatis!
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


//using UnityEngine;

//[RequireComponent(typeof(HingeJoint))]
//public class MekanikJari : MonoBehaviour
//{
//    [Header("Pengaturan Jari")]
//    public float kecepatanPutar = 1f;
//    public string namaBagianTangan = "Jari Tengah";

//    [Header("Perbaikan Arah")]
//    [Tooltip("Centang kotak ini jika saat mouse ditarik ke atas, jari malah ke bawah")]
//    public bool balikArahGerak = false;

//    private Vector2 posisiMouseLama;
//    private HingeJoint engsel;
//    private JointSpring perEngsel;
//    private bool sedangDitarik = false;
//    private Camera kameraUtama;

//    private Vector2 arahTarikanTerkunci;

//    void Start()
//    {
//        engsel = GetComponent<HingeJoint>();
//        engsel.useSpring = true;
//        perEngsel = engsel.spring;

//        perEngsel.spring = 1000f;
//        perEngsel.damper = 50f;
//        engsel.spring = perEngsel;

//        kameraUtama = Camera.main;
//    }

//    void OnMouseEnter()
//    {
//        if (!sedangDitarik && UIManagerTangan.instance != null)
//        {
//            UIManagerTangan.instance.MulaiSorot(namaBagianTangan, transform);
//        }
//    }

//    void OnMouseExit()
//    {
//        if (!sedangDitarik && UIManagerTangan.instance != null)
//            UIManagerTangan.instance.BerhentiSorot();
//    }

//    void OnMouseDown()
//    {
//        sedangDitarik = true;
//        posisiMouseLama = Input.mousePosition;

//        Vector3 sumbuDunia = transform.TransformDirection(engsel.axis);
//        Vector3 posisiLayar = kameraUtama.WorldToScreenPoint(transform.position);
//        Vector3 ujungSumbuLayar = kameraUtama.WorldToScreenPoint(transform.position + sumbuDunia);

//        Vector2 arahSumbuLayar = new Vector2(ujungSumbuLayar.x - posisiLayar.x, ujungSumbuLayar.y - posisiLayar.y).normalized;
//        arahTarikanTerkunci = new Vector2(-arahSumbuLayar.y, arahSumbuLayar.x);

//        if (UIManagerTangan.instance != null)
//            UIManagerTangan.instance.SedangDitarik(namaBagianTangan);
//    }

//    void OnMouseUp()
//    {
//        LepasPegangan();
//    }

//    void OnMouseDrag()
//    {
//        if (!sedangDitarik) return;

//        Vector2 bedaGeser = (Vector2)Input.mousePosition - posisiMouseLama;

//        // Rumus putaran bawaanmu
//        float putaran = Vector2.Dot(bedaGeser, arahTarikanTerkunci) * kecepatanPutar;

//        if (balikArahGerak)
//        {
//            putaran =- putaran;
//        }

//        perEngsel.targetPosition += putaran;

//        if (engsel.useLimits)
//        {
//            perEngsel.targetPosition = Mathf.Clamp(perEngsel.targetPosition, engsel.limits.min, engsel.limits.max);
//        }

//        engsel.spring = perEngsel;
//        posisiMouseLama = Input.mousePosition;
//    }

//    private void LepasPegangan()
//    {
//        if (sedangDitarik)
//        {
//            sedangDitarik = false;
//            if (UIManagerTangan.instance != null)
//                UIManagerTangan.instance.BerhentiSorot();
//        }
//    }
//}