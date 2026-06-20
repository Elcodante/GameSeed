using UnityEngine;

/// <summary>
/// Tempelkan script ini di Main Camera (menggantikan MekanikTangan lama).
///
/// Cara kerja:
/// 1. Saat klik kiri mouse, tembak raycast ke layer "SendiTarik" untuk deteksi Cube (mesh visual) jari mana yang diklik.
/// 2. Selama mouse ditahan & digeser, hitung sudut mouse mengelilingi titik jari itu di layar (seperti memutar kenop).
/// 3. Putar BONE jari yang bersangkutan secara kinematic (langsung set rotasi, tanpa physics/Joint) --
///    karena bone inilah yang menggerakkan mesh visual lewat skinning, bukan Cube itu sendiri.
/// 4. Simpan kecepatan putar tiap jari (index & middle) untuk dibaca HandCrawlController.
///
/// PENTING: Box Collider untuk diklik mouse dipasang di CUBE (mesh visual yang terlihat di layar),
/// BUKAN di bone -- karena bone tidak punya bentuk visual sendiri, posisinya bisa meleset dari
/// bentuk yang terlihat di mata, sehingga klik mouse akan selalu gagal kalau collider dipasang di bone.
/// </summary>
public class MekanikTangan : MonoBehaviour
{
    [Header("Deteksi Klik Jari")]
    [Tooltip("Layer khusus yang berisi Box Collider Cube jari (yang sudah kamu buat sebelumnya)")]
    public LayerMask layerSendi;

    [Tooltip("Jarak maksimum raycast dari kamera")]
    public float jarakRaycast = 100f;

    [Header("Identifikasi Jari Telunjuk (drag manual dari Hierarchy)")]
    [Tooltip("Cube/mesh visual jari telunjuk yang terlihat di layar -- target yang diklik mouse, harus punya Box Collider di layer SendiTarik")]
    public Transform cubeJariTelunjuk;

    [Tooltip("Bone jari telunjuk (misal f_index.03.L) -- ini yang akan diputar oleh script supaya mesh ikut bergerak")]
    public Transform boneJariTelunjuk;

    [Header("Identifikasi Jari Tengah (drag manual dari Hierarchy)")]
    [Tooltip("Cube/mesh visual jari tengah yang terlihat di layar -- target yang diklik mouse, harus punya Box Collider di layer SendiTarik")]
    public Transform cubeJariTengah;

    [Tooltip("Bone jari tengah (misal f_middle.03.L) -- ini yang akan diputar oleh script supaya mesh ikut bergerak")]
    public Transform boneJariTengah;

    [Header("Sensitivitas & Limit Rotasi")]
    public float sensitivity = 1f;
    public float maxCrankSpeed = 720f;
    public float speedDecay = 4f;

    [Header("Rotasi Visual Jari")]
    [Tooltip("Sumbu lokal BONE tempat jari berputar. Coba X dulu, ganti ke Z kalau arah rotasi terasa salah.")]
    public Vector3 rotationAxis = Vector3.right;
    public float maxVisualAngle = 80f;
    public float minVisualAngle = -20f;

    // --- Dibaca oleh HandCrawlController ---
    public float IndexCrankSpeed { get; private set; }
    public float MiddleCrankSpeed { get; private set; }

    // State drag aktif
    private bool _sedangDragIndex;
    private bool _sedangDragMiddle;

    // State sudut & rotasi untuk jari telunjuk
    private float _lastMouseAngleIndex;
    private bool _hasLastAngleIndex;
    private float _visualAngleIndex;
    private Quaternion _initialRotIndex;

    // State sudut & rotasi untuk jari tengah
    private float _lastMouseAngleMiddle;
    private bool _hasLastAngleMiddle;
    private float _visualAngleMiddle;
    private Quaternion _initialRotMiddle;

    void Start()
    {
        if (boneJariTelunjuk != null) _initialRotIndex = boneJariTelunjuk.localRotation;
        if (boneJariTengah != null) _initialRotMiddle = boneJariTengah.localRotation;

        if (cubeJariTelunjuk == null || boneJariTelunjuk == null)
            Debug.LogWarning("[MekanikTangan] cubeJariTelunjuk atau boneJariTelunjuk belum di-assign di Inspector!");
        if (cubeJariTengah == null || boneJariTengah == null)
            Debug.LogWarning("[MekanikTangan] cubeJariTengah atau boneJariTengah belum di-assign di Inspector!");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, jarakRaycast, layerSendi))
            {
                Debug.Log("MANTAP! Laser mengenai jari: " + hit.collider.gameObject.name);

                // Cocokkan objek yang diklik dengan salah satu Cube acuan (termasuk child-nya, kalau ada)
                _sedangDragIndex = cubeJariTelunjuk != null && IsBagianDari(hit.transform, cubeJariTelunjuk);
                _sedangDragMiddle = cubeJariTengah != null && IsBagianDari(hit.transform, cubeJariTengah);

                _hasLastAngleIndex = false;
                _hasLastAngleMiddle = false;
            }
            else
            {
                Debug.Log("Gagal! Laser tidak menabrak layer SendiTarik di kursor ini.");
                _sedangDragIndex = false;
                _sedangDragMiddle = false;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            _sedangDragIndex = false;
            _sedangDragMiddle = false;
        }

        // Hitung rotasi melingkar selama mouse ditahan, lalu putar BONE yang sesuai
        if (Input.GetMouseButton(0))
        {
            if (_sedangDragIndex && boneJariTelunjuk != null)
            {
                ProsesDragJari(
                    boneJariTelunjuk,
                    ref _lastMouseAngleIndex,
                    ref _hasLastAngleIndex,
                    ref _visualAngleIndex,
                    _initialRotIndex,
                    out float speed
                );
                IndexCrankSpeed = speed;
            }

            if (_sedangDragMiddle && boneJariTengah != null)
            {
                ProsesDragJari(
                    boneJariTengah,
                    ref _lastMouseAngleMiddle,
                    ref _hasLastAngleMiddle,
                    ref _visualAngleMiddle,
                    _initialRotMiddle,
                    out float speed
                );
                MiddleCrankSpeed = speed;
            }
        }

        // Peluruhan kecepatan saat tidak di-drag (supaya tangan tidak berhenti mendadak)
        if (!_sedangDragIndex)
            IndexCrankSpeed = Mathf.MoveTowards(IndexCrankSpeed, 0f, speedDecay * maxCrankSpeed * Time.deltaTime / 10f);
        if (!_sedangDragMiddle)
            MiddleCrankSpeed = Mathf.MoveTowards(MiddleCrankSpeed, 0f, speedDecay * maxCrankSpeed * Time.deltaTime / 10f);
    }

    /// <summary>
    /// Cek apakah sebuah transform adalah cubeAcuan itu sendiri ATAU salah satu child-nya.
    /// </summary>
    bool IsBagianDari(Transform target, Transform cubeAcuan)
    {
        Transform current = target;
        while (current != null)
        {
            if (current == cubeAcuan) return true;
            current = current.parent;
        }
        return false;
    }

    /// <summary>
    /// Hitung delta sudut mouse mengelilingi titik BONE jari di screen space,
    /// lalu putar bone itu (kinematic) sesuai delta sudutnya.
    /// </summary>
    void ProsesDragJari(
        Transform bone,
        ref float lastMouseAngle,
        ref bool hasLastAngle,
        ref float visualAngle,
        Quaternion initialRot,
        out float crankSpeed)
    {
        crankSpeed = 0f;

        // Pivot putaran diambil dari posisi BONE (bukan Cube), karena bone adalah pusat rotasi sebenarnya
        Vector2 pivotScreenPos = Camera.main.WorldToScreenPoint(bone.position);
        Vector2 mouseScreenPos = Input.mousePosition;
        Vector2 dir = mouseScreenPos - pivotScreenPos;

        if (dir.sqrMagnitude < 4f) return; // terlalu dekat ke pivot, sudut tidak stabil

        float currentAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (hasLastAngle)
        {
            float deltaAngle = Mathf.DeltaAngle(lastMouseAngle, currentAngle);
            crankSpeed = Mathf.Clamp((deltaAngle / Time.deltaTime) * sensitivity, -maxCrankSpeed, maxCrankSpeed);

            visualAngle = Mathf.Clamp(visualAngle + deltaAngle, minVisualAngle, maxVisualAngle);
            bone.localRotation = initialRot * Quaternion.AngleAxis(visualAngle, rotationAxis);
        }

        lastMouseAngle = currentAngle;
        hasLastAngle = true;
    }
}