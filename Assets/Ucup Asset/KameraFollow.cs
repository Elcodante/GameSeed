using UnityEngine;

public class KameraFollow : MonoBehaviour
{
    // Ini kotak untuk memasukkan objek tangan yang mau diikuti
    public Transform target; 
    
    // Ini untuk mengingat jarak awal antara kamera dan tangan
    private Vector3 jarak;

    void Start()
    {
        // Waktu game dimulai, kamera menghitung jaraknya dengan tangan
        if (target != null)
        {
            jarak = transform.position - target.position;
        }
    }

    // Kita pakai LateUpdate (bukan Update biasa)
    // Fungsinya agar kamera menunggu tangan selesai bergerak dulu, 
    // baru kameranya ikut maju. Ini mencegah layar bergetar/jitter!
    void LateUpdate()
    {
        if (target != null)
        {
            // Kamera berpindah mengikuti posisi tangan + jarak awal
            // Perhatikan: Kita TIDAK mengubah transform.rotation, 
            // jadi layar akan SELALU TEGAK!
            transform.position = target.position + jarak;
        }
    }
}