using UnityEngine;

/// <summary>
/// Tempel di GameObject Player yang menggunakan CharacterController (bukan Rigidbody).
/// Komponen ini menampung "external velocity" dari trap/efek luar lalu meredamnya
/// secara bertahap (drag), supaya gerakan dorongan terasa natural.
///
/// Kalau player controller kamu custom, panggil CurrentExternalVelocity di script
/// movement-mu sendiri dan tambahkan ke hasil akhir CharacterController.Move().
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerPushReceiver : MonoBehaviour, IPushable
{
    [Tooltip("Seberapa cepat dorongan meredam ke nol. Makin besar, makin cepat berhenti.")]
    [SerializeField] private float drag = 4f;

    [Tooltip("Centang kalau script ini yang menggerakkan CharacterController secara mandiri. " +
             "Matikan kalau movement script lain sudah memanggil CharacterController.Move()" +
             " setiap frame, lalu ambil nilai dari CurrentExternalVelocity secara manual.")]
    [SerializeField] private bool autoApplyMovement = true;

    private Vector3 _externalVelocity;
    private CharacterController _cc;

    public Vector3 CurrentExternalVelocity => _externalVelocity;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    public void ApplyPush(Vector3 force)
    {
        _externalVelocity += force;
    }

    private void Update()
    {
        if (_externalVelocity.sqrMagnitude <= 0.0001f)
        {
            _externalVelocity = Vector3.zero;
            return;
        }

        if (autoApplyMovement)
        {
            _cc.Move(_externalVelocity * Time.deltaTime);
        }

        _externalVelocity = Vector3.Lerp(_externalVelocity, Vector3.zero, drag * Time.deltaTime);
    }

    /// <summary>
    /// Panggil ini dari movement script-mu sendiri tiap frame kalau autoApplyMovement = false,
    /// supaya dorongan trap tidak bentrok dengan logika gerak utama.
    /// </summary>
    public void ConsumeExternalVelocity(out Vector3 velocity)
    {
        velocity = _externalVelocity;
        _externalVelocity = Vector3.Lerp(_externalVelocity, Vector3.zero, drag * Time.deltaTime);
    }
}
