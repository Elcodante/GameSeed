using UnityEngine;

/// <summary>
/// Tempelkan script ini di tiap ruas jari yang punya Box Collider (misal f_index.03.L atau f_middle.03.L).
/// Pemain klik & drag mouse mengelilingi jari ini secara melingkar (seperti memutar engkol/kenop),
/// dan script ini akan menghitung seberapa cepat & ke arah mana putaran itu terjadi (CrankSpeed),
/// lalu memutar bone jari ini sesuai arah putaran sebagai feedback visual.
/// </summary>
public class FingerCrank : MonoBehaviour
{
    [Header("Pivot Rotasi Jari")]
    [Tooltip("Sumbu lokal tempat jari ini berputar (biasanya X atau Z, sesuaikan dengan rig). Default: X")]
    public Vector3 rotationAxis = Vector3.right;

    [Header("Sensitivitas & Limit")]
    [Tooltip("Mengalikan delta sudut mentah jadi CrankSpeed. Naikkan jika terasa kurang responsif.")]
    public float sensitivity = 1f;

    [Tooltip("Batas kecepatan putar maksimum (mencegah lonjakan saat mouse melompat jauh antar frame)")]
    public float maxCrankSpeed = 720f; // derajat per detik

    [Tooltip("Seberapa cepat CrankSpeed meluruh ke 0 saat mouse berhenti/dilepas (per detik)")]
    public float speedDecay = 4f;

    [Header("Rotasi Visual Jari")]
    [Tooltip("Apakah bone jari ini ikut berputar secara visual sesuai drag")]
    public bool rotateVisual = true;

    [Tooltip("Batas rotasi visual jari dari posisi awal, derajat (cegah jari memutar penuh 360 yang aneh secara anatomi)")]
    public float maxVisualAngle = 80f;

    [Tooltip("Batas rotasi visual minimum (boleh negatif untuk arah berlawanan)")]
    public float minVisualAngle = -20f;

    // --- Nilai yang dibaca oleh HandCrawlController ---
    /// <summary>Kecepatan putar engkol saat ini (derajat/detik). Positif = satu arah, negatif = arah berlawanan.</summary>
    public float CrankSpeed { get; private set; }

    /// <summary>True selagi pemain sedang drag jari ini.</summary>
    public bool IsDragging { get; private set; }

    private Camera _cam;
    private Quaternion _initialLocalRotation;
    private float _currentVisualAngle;
    private float _lastMouseAngle;
    private bool _hasLastAngle;

    private void Start()
    {
        _cam = Camera.main;
        _initialLocalRotation = transform.localRotation;

        if (_cam == null)
        {
            Debug.LogWarning($"[FingerCrank] Main Camera tidak ditemukan. Pastikan kamera ditandai 'MainCamera'. ({name})");
        }

        // DEBUG SEMENTARA: pastikan collider benar-benar ada dan ukurannya wajar
        var col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"[FingerCrank] TIDAK ADA Collider di objek ini! ({name})");
        }
        else
        {
            Debug.Log($"[FingerCrank] Collider ditemukan di {name}: bounds = {col.bounds}, enabled = {col.enabled}");
        }
    }

    private void OnMouseDown()
    {
        Debug.Log($"[FingerCrank] OnMouseDown terpanggil di {name}"); // DEBUG SEMENTARA
        IsDragging = true;
        _hasLastAngle = false; // reset, supaya delta sudut dihitung ulang dari titik drag baru
    }

    private void OnMouseDrag()
    {
        Debug.Log($"[FingerCrank] OnMouseDrag terpanggil di {name}"); // DEBUG SEMENTARA
        if (_cam == null) return;

        // Proyeksikan posisi pivot jari ini ke screen space, jadi titik pusat putaran di layar
        Vector2 pivotScreenPos = _cam.WorldToScreenPoint(transform.position);
        Vector2 mouseScreenPos = Input.mousePosition;

        Vector2 dir = mouseScreenPos - pivotScreenPos;

        // Hindari hitung sudut saat mouse terlalu dekat dengan pivot (sudut jadi tidak stabil)
        if (dir.sqrMagnitude < 4f) return;

        float currentAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (_hasLastAngle)
        {
            float deltaAngle = Mathf.DeltaAngle(_lastMouseAngle, currentAngle);
            float rawSpeed = (deltaAngle / Time.deltaTime) * sensitivity;
            CrankSpeed = Mathf.Clamp(rawSpeed, -maxCrankSpeed, maxCrankSpeed);

            if (rotateVisual)
            {
                _currentVisualAngle = Mathf.Clamp(
                    _currentVisualAngle + deltaAngle,
                    minVisualAngle,
                    maxVisualAngle
                );
                transform.localRotation = _initialLocalRotation * Quaternion.AngleAxis(_currentVisualAngle, rotationAxis);
            }
        }

        _lastMouseAngle = currentAngle;
        _hasLastAngle = true;
    }

    private void OnMouseUp()
    {
        IsDragging = false;
        _hasLastAngle = false;
    }

    private void Update()
    {
        // Saat tidak di-drag, kecepatan meluruh perlahan ke 0 (bukan langsung berhenti, biar tidak kasar)
        if (!IsDragging)
        {
            CrankSpeed = Mathf.MoveTowards(CrankSpeed, 0f, speedDecay * maxCrankSpeed * Time.deltaTime / 10f);
        }
    }

    /// <summary>
    /// Reset rotasi visual jari kembali ke pose awal (berguna untuk restart level / debug).
    /// </summary>
    public void ResetVisual()
    {
        _currentVisualAngle = 0f;
        transform.localRotation = _initialLocalRotation;
        CrankSpeed = 0f;
        IsDragging = false;
        _hasLastAngle = false;
    }
}