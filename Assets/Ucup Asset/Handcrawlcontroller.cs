using UnityEngine;

/// <summary>
/// Tempelkan script ini di objek tangan utama (parent) yang memiliki Rigidbody + Collider.
/// Script ini mengambil CrankSpeed dari dua FingerCrank (jari telunjuk & jari tengah),
/// menggabungkannya, lalu menggerakkan tangan maju seperti roda yang diputar dua pedal.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class HandCrawlController : MonoBehaviour
{
    [Header("Referensi Mekanik Tangan")]
    [Tooltip("Drag Main Camera ke sini (objek yang punya script MekanikTangan)")]
    public MekanikTangan mekanikTangan;

    [Header("Konversi Putaran -> Gerak Maju")]
    [Tooltip("Mengubah rata-rata CrankSpeed (derajat/detik) jadi kecepatan maju (unit/detik). " +
             "Misal: 1 putaran penuh (360 derajat) menghasilkan sekian unit maju.")]
    public float speedToVelocityFactor = 0.02f;

    [Tooltip("Kecepatan maju maksimum tangan, untuk mencegah meluncur terlalu kencang")]
    public float maxForwardSpeed = 3f;

    [Header("Syarat Gerak (opsional, sesuai gaya dual-crank)")]
    [Tooltip("Jika true: kedua jari HARUS diputar searah (sama-sama positif atau sama-sama negatif) " +
             "untuk menghasilkan gerak maju -- meniru cara kerja dua pedal sepeda yang harus berlawanan fase. " +
             "Jika false: rata-rata speed langsung dipakai apa adanya.")]
    public bool requireAlternatingCranks = false;

    [Header("Stabilitas Fisik")]
    [Tooltip("Kunci rotasi X & Z Rigidbody supaya tangan tidak terguling saat merayap")]
    public bool lockTiltRotation = true;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        if (lockTiltRotation)
        {
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        if (mekanikTangan == null)
        {
            Debug.LogWarning("[HandCrawlController] mekanikTangan belum di-assign di Inspector!");
        }
    }

    private void FixedUpdate()
    {
        if (mekanikTangan == null) return;

        float indexSpeed = mekanikTangan.IndexCrankSpeed;
        float middleSpeed = mekanikTangan.MiddleCrankSpeed;

        float combinedSpeed;

        if (requireAlternatingCranks)
        {
            // Gaya pedal sepeda: kedua jari harus diputar dengan arah berlawanan tanda
            // (satu positif, satu negatif) untuk menghasilkan gerak maju efektif.
            bool oppositeDirection = Mathf.Sign(indexSpeed) != Mathf.Sign(middleSpeed) &&
                                      Mathf.Abs(indexSpeed) > 1f && Mathf.Abs(middleSpeed) > 1f;

            combinedSpeed = oppositeDirection
                ? (Mathf.Abs(indexSpeed) + Mathf.Abs(middleSpeed)) * 0.5f
                : 0f;
        }
        else
        {
            // Gaya sederhana: rata-rata kecepatan absolut keduanya (putar searah apapun = maju)
            combinedSpeed = (Mathf.Abs(indexSpeed) + Mathf.Abs(middleSpeed)) * 0.5f;
        }

        float forwardSpeed = Mathf.Clamp(combinedSpeed * speedToVelocityFactor, 0f, maxForwardSpeed);

        // Gerakkan tangan maju mengikuti arah hadap transform-nya sendiri,
        // tapi pertahankan komponen vertikal (Y) dari velocity yang sudah ada (gravitasi/lompat)
        Vector3 forwardDir = transform.forward;
        Vector3 horizontalVelocity = forwardDir * forwardSpeed;

        Vector3 currentVel = _rb.linearVelocity;
        _rb.linearVelocity = new Vector3(horizontalVelocity.x, currentVel.y, horizontalVelocity.z);
    }

    /// <summary>
    /// Helper untuk debug di Inspector / on-screen text, menampilkan kecepatan maju saat ini.
    /// </summary>
    public float GetCurrentForwardSpeed()
    {
        return _rb != null ? new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z).magnitude : 0f;
    }
}