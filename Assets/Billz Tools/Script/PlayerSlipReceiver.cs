using UnityEngine;

/// <summary>
/// Tempel di Player yang menggunakan CharacterController (bukan Rigidbody).
/// Menerima perintah gelincir dari SlipperyFloor dan mengakumulasi slide velocity
/// secara gradual — Player tidak akan langsung kencang, melainkan perlahan makin cepat.
///
/// Pose / rotasi Player TIDAK diubah sama sekali.
/// Hanya posisi (via CharacterController.Move) yang bergerak.
///
/// Integrasi dengan movement script lain:
///   - autoApplyMovement = true  → komponen ini yang panggil CharacterController.Move() untuk slide.
///     Movement utama (input player) tetap dihandle script terpisah, keduanya tidak bentrok
///     karena CharacterController.Move() bisa dipanggil beberapa kali per frame secara aman.
///   - autoApplyMovement = false → ambil nilai dari CurrentSlideVelocity dan
///     tambahkan sendiri ke dalam panggilan Move() di movement script utama.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerSlipReceiver : MonoBehaviour, ISlippable
{
    [Tooltip("Lihat komentar di atas. Aktifkan kalau tidak ada script lain yang perlu mengkonsumsi CurrentSlideVelocity.")]
    [SerializeField] private bool autoApplyMovement = true;

    // ── Public read-only state ─────────────────────────────────────────────────
    /// <summary>Velocity gelincir saat ini dalam world space. Bisa dibaca movement script lain.</summary>
    public Vector3  CurrentSlideVelocity => _slideVelocity;
    /// <summary>True selama Player berada di atas lantai licin.</summary>
    public bool     IsOnSlipperyFloor    => _isSliding;

    // ── Private state ──────────────────────────────────────────────────────────
    private CharacterController _cc;

    private Vector3 _slideVelocity;
    private Vector3 _slideDir;
    private float   _acceleration;
    private float   _maxSpeed;
    private float   _brakingDecel;
    private bool    _isSliding;

    private void Awake() => _cc = GetComponent<CharacterController>();

    // ── ISlippable ─────────────────────────────────────────────────────────────
    public void BeginSlide(Vector3 worldSlideDir, float acceleration,
                           float maxSpeed, float brakingDeceleration)
    {
        _slideDir    = worldSlideDir;
        _acceleration = acceleration;
        _maxSpeed     = maxSpeed;
        _brakingDecel = brakingDeceleration;
        _isSliding    = true;
    }

    public void EndSlide()
    {
        _isSliding = false;
        // _slideVelocity TIDAK di-reset di sini —
        // Player masih meluncur sebentar setelah keluar (efek momentum es).
    }

    // ── Update ─────────────────────────────────────────────────────────────────
    private void Update()
    {
        if (_isSliding)
            AccelerateSlide();
        else
            BrakeSlide();

        if (autoApplyMovement && _slideVelocity.sqrMagnitude > 0.0001f)
            _cc.Move(_slideVelocity * Time.deltaTime);
    }

    private void AccelerateSlide()
    {
        // Seberapa besar komponen velocity yang sudah ada di arah gelincir
        float currentSpeedOnDir = Vector3.Dot(_slideVelocity, _slideDir);
        if (currentSpeedOnDir >= _maxSpeed) return;

        float delta = Mathf.Min(_acceleration * Time.deltaTime,
                                _maxSpeed - currentSpeedOnDir);
        _slideVelocity += _slideDir * delta;
    }

    private void BrakeSlide()
    {
        if (_slideVelocity.sqrMagnitude < 0.0001f)
        {
            _slideVelocity = Vector3.zero;
            return;
        }
        _slideVelocity = Vector3.MoveTowards(
            _slideVelocity, Vector3.zero, _brakingDecel * Time.deltaTime);
    }

    /// <summary>
    /// Gunakan metode ini kalau autoApplyMovement = false dan movement script lain
    /// yang meng-handle CharacterController.Move(). Panggil tiap frame, ambil nilainya,
    /// tambahkan ke final move vector-mu.
    /// </summary>
    public Vector3 ConsumeSlideVelocity()
    {
        Vector3 v = _slideVelocity;
        // Tetap jalankan brake/accelerate meski dikonsumsi dari luar
        if (_isSliding) AccelerateSlide(); else BrakeSlide();
        return v;
    }
}
