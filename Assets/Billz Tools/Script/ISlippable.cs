/// <summary>
/// Implement interface ini di komponen Player yang menggunakan CharacterController
/// (bukan Rigidbody) agar bisa dipengaruhi efek lantai licin dari SlipperyFloor.
///
/// Kalau Player sudah pakai Rigidbody, SlipperyFloor menangani sendiri via
/// drag + AddForce tanpa perlu interface ini.
/// </summary>
public interface ISlippable
{
    /// <summary>Dipanggil saat Player MASUK ke area lantai licin.</summary>
    void BeginSlide(UnityEngine.Vector3 worldSlideDir, float acceleration,
                    float maxSpeed, float brakingDeceleration);

    /// <summary>Dipanggil saat Player KELUAR dari area lantai licin.
    /// Slide velocity tidak langsung nol — melambat bertahap sesuai brakingDeceleration.</summary>
    void EndSlide();
}
