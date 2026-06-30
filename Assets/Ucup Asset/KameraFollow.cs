using UnityEngine;

public class KameraFollow : MonoBehaviour
{
    [Header("Target Kamera")]
    public Transform target;

    [Header("Kecepatan & Kehalusan")]
    public float kecepatanPutar = 60f;
    public float kecepatanZoom = 2f;     // Kecepatan scroll diturunkan agar lebih presisi
    public float kehalusanZoom = 10f;    // Semakin kecil angkanya, semakin lambat/mulus meluncurnya

    [Header("Batas Zoom (Jarak)")]
    public float jarakMinimal = 0.8f;    // Disesuaikan dengan rentang yang kamu inginkan
    public float jarakMaksimal = 5f;

    [Header("Batas Rotasi Atas/Bawah")]
    public float batasBawahY = 5f;
    public float batasAtasY = 85f;

    private float yaw = 0f;
    private float pitch = 30f;
    private float jarakSaatIni = 5f;     // Posisi asli kamera saat ini
    private float targetJarak = 5f;      // Posisi tujuan zoom dari scroll mouse

    void Start()
    {
        if (target != null)
        {
            jarakSaatIni = Vector3.Distance(transform.position, target.position);
            targetJarak = jarakSaatIni; // Samakan target awal dengan posisi awal
        }
    }

    void Update()
    {
        if (target == null) return;

        // 1. Deteksi Input Tombol WASD
        float inputX = 0f;
        float inputY = 0f;

        if (Input.GetKey(KeyCode.D)) inputX = 1f;
        if (Input.GetKey(KeyCode.A)) inputX = -1f;
        if (Input.GetKey(KeyCode.W)) inputY = 1f;
        if (Input.GetKey(KeyCode.S)) inputY = -1f;

        yaw += inputX * kecepatanPutar * Time.deltaTime;
        pitch += inputY * kecepatanPutar * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, batasBawahY, batasAtasY);

        // 2. Deteksi Input Zoom (Scroll Mouse)
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            // Scroll mengubah TARGET jarak, bukan langsung jaraknya
            targetJarak -= scroll * kecepatanZoom;
            targetJarak = Mathf.Clamp(targetJarak, jarakMinimal, jarakMaksimal);
        }

        // 3. Muluskan (Lerp) Jarak Saat Ini menuju Target Jarak
        jarakSaatIni = Mathf.Lerp(jarakSaatIni, targetJarak, Time.deltaTime * kehalusanZoom);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Kalkulasi Posisi Kamera Orbit
        Quaternion rotasi = Quaternion.Euler(pitch, yaw, 0);
        Vector3 posisiBaru = target.position - (rotasi * Vector3.forward * jarakSaatIni);

        transform.position = posisiBaru;
        transform.LookAt(target);
    }
}