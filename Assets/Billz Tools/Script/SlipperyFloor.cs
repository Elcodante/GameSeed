using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Lantai licin: Player yang menginjak area ini akan perlahan tergelincir,
/// bahkan jika sedang diam. Pose / rotasi Player tidak diubah sama sekali.
///
/// Tempel di GameObject lantai yang punya Collider dengan isTrigger = true.
/// Mendukung Player berbasis Rigidbody maupun CharacterController (via ISlippable).
/// </summary>
[DisallowMultipleComponent]
public class SlipperyFloor : MonoBehaviour
{
    public enum SlideDirMode
    {
        FixedWorldDirection,    // arah gelincir tetap, diatur manual (Vector3)
        FollowFloorSlope,       // otomatis ikut kemiringan lantai (tilt object di Inspector → arahnya ikut)
        FollowPlayerMomentum    // ikut arah gerak Player saat menginjak; kalau diam, fallback ke FixedWorldDirection
    }

    [Header("Deteksi")]
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private LayerMask detectionLayer = ~0;

    [Header("Arah Gelincir")]
    [SerializeField] private SlideDirMode slideDirMode = SlideDirMode.FixedWorldDirection;
    [Tooltip("Dipakai untuk FixedWorldDirection, dan sebagai fallback kalau Player diam di FollowPlayerMomentum")]
    [SerializeField] private Vector3 fixedSlideDirection = Vector3.forward;

    [Header("Feel Gelincir")]
    [Tooltip("Percepatan gelincir (m/s²). Coba mulai dari 3-5.")]
    [SerializeField] private float acceleration = 4f;
    [Tooltip("Kecepatan gelincir maksimum (m/s).")]
    [SerializeField] private float maxSlideSpeed = 6f;
    [Tooltip("Seberapa cepat berhenti setelah keluar lantai licin.")]
    [SerializeField] private float brakingDeceleration = 3f;
    [Tooltip("Buang velocity tegak lurus arah slide saat Player pertama masuk. " +
             "AKTIFKAN agar Player selalu meluncur lurus sesuai arah slide apapun arah datangnya. " +
             "Matikan kalau kamu mau momentum diagonal dipertahankan.")]
    [SerializeField] private bool stripPerpendicularMomentumOnEnter = true;

    [Header("Rigidbody Settings")]
    [Tooltip("Drag saat di lantai licin. Nilai sangat kecil (0.01–0.1) = makin licin.")]
    [SerializeField] private float slipperyDrag = 0.05f;
    [Tooltip("Bekukan rotasi X dan Z agar Player tidak 'njungkil' akibat fisika.")]
    [SerializeField] private bool freezeRotationOnSlide = true;

    [Header("Debug")]
    [Tooltip("Centang untuk lihat log deteksi di Console saat play mode.")]
    [SerializeField] private bool debugLog = true;

    [Header("Events")]
    public UnityEvent OnSlideBegin;
    public UnityEvent OnSlideEnd;

    // ── State ──────────────────────────────────────────────────────────────────
    private Collider _trackedPlayer;
    private Rigidbody _playerRb;
    private ISlippable _slippable;

    private float _originalDrag;
    private bool _wasFreezingRotX;
    private bool _wasFreezingRotY;
    private bool _wasFreezingRotZ;
    private Vector3 _cachedSlideDir;

    // ── Init ───────────────────────────────────────────────────────────────────
    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    // ── Trigger ────────────────────────────────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (_trackedPlayer != null) return;
        if (((1 << other.gameObject.layer) & detectionLayer) == 0) return;
        if (!other.CompareTag(targetTag)) return;

        _trackedPlayer = other;
        _playerRb = other.attachedRigidbody;
        _slippable = other.GetComponentInParent<ISlippable>();
        _cachedSlideDir = ResolveSlideDirection(other);

        // Buang komponen velocity yang tegak lurus arah slide agar Player tidak meluncur diagonal
        // saat masuk dari sudut yang berbeda. Velocity di arah slide dan sumbu Y tetap dipertahankan.
        if (stripPerpendicularMomentumOnEnter && _playerRb != null)
        {
            Vector3 vel = _playerRb.linearVelocity;
            // Sumbu tegak lurus slide di bidang horizontal
            Vector3 perp = Vector3.Cross(_cachedSlideDir, Vector3.up);
            // Hapus komponen perpendicular, pertahankan slide + vertical
            vel -= perp * Vector3.Dot(vel, perp);
            _playerRb.linearVelocity = vel;
        }

        if (debugLog)
        {
            string mode = _playerRb != null ? "Rigidbody" :
                          _slippable != null ? "CharacterController (ISlippable)" :
                          "TIDAK ADA Rigidbody maupun PlayerSlipReceiver — slide tidak akan berfungsi!";
            Debug.Log($"[SlipperyFloor] '{other.name}' masuk. Mode: {mode}. " +
                      $"Arah slide: {_cachedSlideDir}");
        }

        if (_slippable != null)
        {
            _slippable.BeginSlide(_cachedSlideDir, acceleration, maxSlideSpeed, brakingDeceleration);
        }
        else if (_playerRb != null)
        {
            _originalDrag = _playerRb.linearDamping;
            _playerRb.linearDamping = slipperyDrag;

            if (freezeRotationOnSlide)
            {
                // Cache state semua sumbu rotasi sebelum kita ubah
                _wasFreezingRotX = (_playerRb.constraints & RigidbodyConstraints.FreezeRotationX) != 0;
                _wasFreezingRotY = (_playerRb.constraints & RigidbodyConstraints.FreezeRotationY) != 0;
                _wasFreezingRotZ = (_playerRb.constraints & RigidbodyConstraints.FreezeRotationZ) != 0;
                // Freeze SEMUA sumbu — X/Z mencegah miring, Y mencegah spin/berputar di tempat
                _playerRb.constraints |= RigidbodyConstraints.FreezeRotation;
            }
        }
        else
        {
            // Tidak ada Rigidbody maupun ISlippable — ingatkan developer
            Debug.LogWarning($"[SlipperyFloor] '{other.name}' tidak punya Rigidbody " +
                             "maupun komponen PlayerSlipReceiver. " +
                             "Tambahkan salah satu agar slide berfungsi.");
        }

        OnSlideBegin?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (_trackedPlayer != other) return;

        if (debugLog)
            Debug.Log($"[SlipperyFloor] '{other.name}' keluar dari lantai licin.");

        if (_slippable != null)
        {
            _slippable.EndSlide();
        }
        else if (_playerRb != null)
        {
            _playerRb.linearDamping = _originalDrag;

            if (freezeRotationOnSlide)
            {
                // Kembalikan hanya sumbu yang memang bebas sebelumnya
                if (!_wasFreezingRotX) _playerRb.constraints &= ~RigidbodyConstraints.FreezeRotationX;
                if (!_wasFreezingRotY) _playerRb.constraints &= ~RigidbodyConstraints.FreezeRotationY;
                if (!_wasFreezingRotZ) _playerRb.constraints &= ~RigidbodyConstraints.FreezeRotationZ;
            }
        }

        OnSlideEnd?.Invoke();
        _trackedPlayer = null;
        _playerRb = null;
        _slippable = null;
    }

    // ── Physics loop ───────────────────────────────────────────────────────────
    private void FixedUpdate()
    {
        if (_playerRb == null || _trackedPlayer == null) return;

        float speedAlongSlide = Vector3.Dot(_playerRb.linearVelocity, _cachedSlideDir);
        if (speedAlongSlide >= maxSlideSpeed) return;

        // FIX: ForceMode.Acceleration sudah otomatis dikalikan mass dan delta time oleh PhysX.
        // Jangan kalikan Time.fixedDeltaTime manual di sini — itu yang bikin gaya jadi hampir nol.
        _playerRb.AddForce(_cachedSlideDir * acceleration, ForceMode.Acceleration);
    }

    // ── Direction resolver ─────────────────────────────────────────────────────
    private Vector3 ResolveSlideDirection(Collider player)
    {
        switch (slideDirMode)
        {
            case SlideDirMode.FollowFloorSlope:
                return CalcSlopeDirection();

            case SlideDirMode.FollowPlayerMomentum:
                if (player.attachedRigidbody != null)
                {
                    Vector3 v = player.attachedRigidbody.linearVelocity;
                    Vector3 h = new Vector3(v.x, 0f, v.z);
                    if (h.sqrMagnitude > 0.04f) return h.normalized;
                }
                return fixedSlideDirection.normalized; // fallback kalau diam

            default: // FixedWorldDirection
                return fixedSlideDirection.normalized;
        }
    }

    private Vector3 CalcSlopeDirection()
    {
        Vector3 normal = transform.up;
        Vector3 gravDir = Physics.gravity.normalized;
        Vector3 slope = gravDir - Vector3.Dot(gravDir, normal) * normal;
        return slope.sqrMagnitude > 0.001f ? slope.normalized : transform.forward;
    }

    // ── Gizmo ──────────────────────────────────────────────────────────────────
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 dir = slideDirMode == SlideDirMode.FollowFloorSlope
            ? CalcSlopeDirection()
            : fixedSlideDirection.normalized;

        Gizmos.color = new Color(0.3f, 0.85f, 1f, 0.9f);
        Vector3 origin = transform.position + transform.up * 0.02f;
        Gizmos.DrawLine(origin, origin + dir * 2f);
        Gizmos.DrawSphere(origin + dir * 2f, 0.08f);

        Bounds bounds = GetComponent<Collider>()?.bounds ?? new Bounds(transform.position, Vector3.one);
        Vector3 stripeAxis = Vector3.Cross(dir, transform.up).normalized;
        float halfW = Mathf.Abs(Vector3.Dot(bounds.size, stripeAxis)) * 0.5f;
        float halfD = Mathf.Abs(Vector3.Dot(bounds.size, dir)) * 0.5f;
        Vector3 center = bounds.center + transform.up * 0.02f;

        UnityEditor.Handles.color = new Color(0.3f, 0.85f, 1f, 0.25f);
        for (int i = 0; i <= 6; i++)
        {
            float t = Mathf.Lerp(-halfW, halfW, (float)i / 6f);
            Vector3 offset = stripeAxis * t;
            UnityEditor.Handles.DrawLine(
                center + offset - dir * halfD,
                center + offset + dir * halfD);
        }
    }
#endif
}