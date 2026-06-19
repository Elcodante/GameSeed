/// <summary>
/// Implement interface ini di komponen apapun yang harus bisa "didorong" oleh trap,
/// tapi TIDAK punya Rigidbody (misalnya karakter berbasis CharacterController).
/// Kalau objekmu sudah pakai Rigidbody, kamu TIDAK perlu interface ini sama sekali,
/// TrapPushback akan otomatis pakai Rigidbody.AddForce().
/// </summary>
public interface IPushable
{
    void ApplyPush(UnityEngine.Vector3 force);
}
