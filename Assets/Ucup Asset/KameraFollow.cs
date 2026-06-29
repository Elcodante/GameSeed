using UnityEngine;

public class KameraFollow : MonoBehaviour
{
    // Kotak untuk memasukkan objek induk tangan (Tangan V1)
    public Transform target;

    // Kecepatan gerak dan zoom, bisa diatur di Inspector Unity
    public float kecepatanKamera = 10f;
    public float kecepatanZoom = 20f;

    // Batas jarak agar kamera tidak menabrak / menembus tangan
    public float batasDekat = 3f;

    void Update()
    {
        float gerakKiriKanan = 0f;
        float gerakAtasBawah = 0f;

        if (Input.GetKey(KeyCode.W)) gerakAtasBawah = 1f;
        if (Input.GetKey(KeyCode.S)) gerakAtasBawah = -1f;
        if (Input.GetKey(KeyCode.D)) gerakKiriKanan = 1f;
        if (Input.GetKey(KeyCode.A)) gerakKiriKanan = -1f;

        // W dan S = Bergerak lurus ke Atas dan Bawah dunia game
        Vector3 geserAtasBawah = Vector3.up * gerakAtasBawah;

        // A dan D = Bergerak ke Kiri dan Kanan mengikuti sudut pandang lensa kamera
        Vector3 geserKiriKanan = transform.right * gerakKiriKanan;

        Vector3 arahGerakWASD = geserAtasBawah + geserKiriKanan;
        transform.Translate(arahGerakWASD * kecepatanKamera * Time.deltaTime, Space.World);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f && target != null)
        {
            // Menghitung arah maju (zoom in) dan mundur (zoom out)
            Vector3 arahZoom = transform.forward * scroll * kecepatanZoom * Time.deltaTime;

            float jarakPrediksi = Vector3.Distance(transform.position + arahZoom, target.position);

            if (jarakPrediksi >= batasDekat)
            {
                transform.position += arahZoom;
            }
            else if (scroll > 0f)
            {
                transform.position += Vector3.up * scroll * kecepatanZoom * Time.deltaTime;
            }
        }
    }

    void LateUpdate()
    {
        // Kamera akan selalu fokus memutar lensanya menatap tangan
        if (target != null)
        {
            transform.LookAt(target);
        }
    }
}