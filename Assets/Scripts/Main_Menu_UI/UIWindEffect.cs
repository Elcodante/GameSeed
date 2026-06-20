using UnityEngine;
using UnityEngine.UI; // Wajib untuk mengakses fitur UI dan BaseMeshEffect

// BaseMeshEffect adalah kelas khusus dari Unity untuk memanipulasi bentuk UI
public class UIWindEffect : BaseMeshEffect
{
    [Header("Pengaturan Angin")]
    [Tooltip("Kecepatan kibasan angin")]
    public float waveSpeed = 5f;

    [Tooltip("Seberapa tinggi ujung gambar naik/turun (dalam pixel)")]
    public float waveHeight = 3f;

    [Tooltip("Kerapatan gelombang. Ubah jika gambar terlalu kaku atau terlalu keriting")]
    public float waveFrequency = 0.01f;

    void Update()
    {
        // Memaksa Unity untuk terus memperbarui bentuk gambar setiap frame agar animasinya jalan
        if (graphic != null)
        {
            graphic.SetVerticesDirty();
        }
    }

    // Fungsi ini otomatis dipanggil Unity untuk menggambar ulang UI
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        UIVertex vertex = new UIVertex();

        // Kita mengambil setiap titik (vertex) yang membentuk gambar Sliced UI
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            // Menggunakan rumus matematika gelombang (Sine) berdasarkan waktu dan posisi X (kiri/kanan)
            float wave = Mathf.Sin(Time.time * waveSpeed + (vertex.position.x * waveFrequency));

            // Menerapkan gelombang ke posisi Y (Atas/Bawah) dari titik tersebut
            vertex.position.y += wave * waveHeight;

            vh.SetUIVertex(vertex, i);
        }
    }
}