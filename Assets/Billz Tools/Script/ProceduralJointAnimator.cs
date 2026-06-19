using UnityEngine;

/// <summary>
/// Contoh "driver" yang menggerakkan JointConstraint secara otomatis (osilasi sinus).
/// Pasang di GameObject yang sama dengan JointConstraint.
///
/// Dipakai untuk: pendulum/swinging trap, tentakel yang bergoyang, lengan robot idle, dst.
/// Tinggal atur axis, speed, range, dan phaseOffset (biar tiap sendi di chain tidak gerak
/// barengan persis -> efek gelombang/tentakel natural).
/// </summary>
[RequireComponent(typeof(JointConstraint))]
public class ProceduralJointAnimator : MonoBehaviour
{
    [Tooltip("Sumbu yang dianimasikan, dalam derajat euler local")]
    public Vector3 oscillationAxis = Vector3.up;

    [Tooltip("Besar ayunan dari titik tengah (derajat)")]
    public float amplitude = 30f;

    [Tooltip("Kecepatan osilasi")]
    public float speed = 1f;

    [Tooltip("Geser fase osilasi, berguna untuk efek gelombang berurutan di joint chain")]
    public float phaseOffset = 0f;

    private JointConstraint _joint;
    private float _time;

    private void Awake()
    {
        _joint = GetComponent<JointConstraint>();
    }

    private void Update()
    {
        _time += Time.deltaTime * speed;
        float angle = Mathf.Sin(_time + phaseOffset) * amplitude;
        _joint.SetTargetLocalEuler(oscillationAxis * angle);
    }
}
