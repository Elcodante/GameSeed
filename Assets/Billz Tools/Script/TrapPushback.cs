using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Trap generik yang mendorong objek bertag tertentu (default "Player") menjauh.
/// Tempel di GameObject manapun yang punya Collider dengan isTrigger = true.
/// Semua parameter penting bisa diubah lewat Inspector tanpa sentuh kode.
/// </summary>
[DisallowMultipleComponent]
public class TrapPushback : MonoBehaviour
{
    public enum PushDirectionMode
    {
        TrapForward,        // arah dorong = transform.forward trap (cocok untuk trap dinding/spring panel)
        AwayFromTrapCenter, // arah dorong = menjauh dari posisi trap (cocok untuk trap radial/ranjau)
        CustomDirection     // arah dorong = vector custom yang kamu set sendiri
    }

    [Header("Deteksi")]
    [Tooltip("Tag yang memicu trap ini")]
    [SerializeField] private string targetTag = "Player";
    [Tooltip("Batasi trap hanya bereaksi ke layer tertentu (opsional, biarkan Everything kalau tidak perlu)")]
    [SerializeField] private LayerMask detectionLayer = ~0;

    [Header("Pengaturan Dorongan")]
    [SerializeField] private PushDirectionMode directionMode = PushDirectionMode.TrapForward;
    [SerializeField] private Vector3 customDirection = Vector3.forward;
    [SerializeField] private float pushForce = 15f;
    [SerializeField] private float upwardBoost = 2f;

    [Header("Timing")]
    [Tooltip("Jeda sebelum trap bisa trigger lagi")]
    [SerializeField] private float cooldown = 1f;
    [Tooltip("Kalau dicentang, trap hanya aktif sekali seumur hidup (misal jebakan satu kali pakai)")]
    [SerializeField] private bool oneShot = false;
    [Tooltip("Delay opsional sebelum dorongan benar-benar diberikan, untuk efek 'telegraph' (animasi ancang-ancang)")]
    [SerializeField] private float activationDelay = 0f;

    [Header("Events (hook animasi, sfx, vfx di sini)")]
    public UnityEvent OnTrapTriggered;
    public UnityEvent OnTrapReady;

    private float _cooldownTimer;
    private bool _hasFired;

    private void Reset()
    {
        // Auto-set collider jadi trigger saat pertama kali script ditambahkan, biar tidak lupa
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        // Timer ringan, tidak ada alokasi/GetComponent di sini
        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer <= 0f) OnTrapReady?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (oneShot && _hasFired) return;
        if (_cooldownTimer > 0f) return;
        if (((1 << other.gameObject.layer) & detectionLayer) == 0) return;
        if (!other.CompareTag(targetTag)) return;

        if (activationDelay > 0f)
            StartCoroutine(FireDelayed(other));
        else
            Fire(other);
    }

    private System.Collections.IEnumerator FireDelayed(Collider other)
    {
        yield return new WaitForSeconds(activationDelay);
        // other bisa saja sudah keluar trigger saat delay habis, cek validitasnya
        if (other != null) Fire(other);
    }

    private void Fire(Collider other)
    {
        Vector3 dir = GetPushDirection(other.transform.position);
        ApplyPush(other, dir);

        _cooldownTimer = cooldown;
        _hasFired = true;
        OnTrapTriggered?.Invoke();
    }

    private Vector3 GetPushDirection(Vector3 targetPos)
    {
        switch (directionMode)
        {
            case PushDirectionMode.AwayFromTrapCenter:
                Vector3 d = targetPos - transform.position;
                d.y = 0f; // jaga dorongan tetap horizontal, hapus baris ini kalau mau dorongan 3D penuh
                return d.sqrMagnitude > 0.001f ? d.normalized : transform.forward;

            case PushDirectionMode.CustomDirection:
                return customDirection.normalized;

            default: // TrapForward
                return transform.forward;
        }
    }

    private void ApplyPush(Collider other, Vector3 dir)
    {
        Vector3 force = dir * pushForce + Vector3.up * upwardBoost;

        // Prioritas 1: kalau target punya Rigidbody, pakai physics asli (paling murah & natural)
        if (other.attachedRigidbody != null)
        {
            other.attachedRigidbody.AddForce(force, ForceMode.VelocityChange);
            return;
        }

        // Prioritas 2: kalau tidak ada Rigidbody (misal pakai CharacterController),
        // lempar ke interface IPushable
        var pushable = other.GetComponentInParent<IPushable>();
        pushable?.ApplyPush(force);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 dir = directionMode == PushDirectionMode.CustomDirection
            ? customDirection.normalized
            : transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + dir * 2f);
        Gizmos.DrawWireSphere(transform.position + dir * 2f, 0.1f);
    }
#endif
}
