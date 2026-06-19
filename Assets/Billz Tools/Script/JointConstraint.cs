using UnityEngine;

/// <summary>
/// "Sendi" ringan berbasis Transform (BUKAN PhysX joint), jadi jauh lebih murah daripada
/// ConfigurableJoint/CharacterJoint bawaan Unity. Cocok untuk lengan robot, tentakel,
/// trap berayun (pendulum), ekor, jembatan gantung, dsb.
///
/// Tempel satu komponen ini di setiap "tulang" dalam hierarchy (parent-child chain).
/// Bentuk & limit sendi diatur lewat Inspector, dan langsung kelihatan di Scene view
/// lewat gizmo (kerucut untuk BallSocket, busur untuk Hinge).
/// </summary>
public class JointConstraint : MonoBehaviour
{
    public enum JointType { Hinge, BallSocket, Fixed }
    public enum Axis { X, Y, Z }

    [Header("Tipe & Bentuk Sendi")]
    public JointType jointType = JointType.Hinge;
    [Tooltip("Sumbu putar untuk tipe Hinge saja")]
    public Axis hingeAxis = Axis.Y;

    [Header("Limit Rotasi (derajat, local space)")]
    public Vector2 limitX = new Vector2(-45f, 45f);
    public Vector2 limitY = new Vector2(-45f, 45f);
    public Vector2 limitZ = new Vector2(-45f, 45f);

    [Header("Gerakan")]
    [Tooltip("Kecepatan interpolasi menuju target rotation (derajat/detik). 0 = langsung snap (paling murah)")]
    public float rotationSpeed = 180f;

    [Header("Gizmo (hanya tampil di Editor, tidak ada biaya saat build)")]
    public bool drawGizmo = true;
    public float gizmoSize = 0.2f;
    public Color gizmoColor = new Color(0.2f, 0.8f, 1f, 0.6f);

    private Quaternion _targetLocalRotation;

    private void Awake()
    {
        _targetLocalRotation = transform.localRotation;
    }

    /// <summary>
    /// Set target rotasi sendi pakai euler local. Otomatis di-clamp sesuai limit & tipe joint.
    /// Panggil ini dari script animasi, IK, AI, atau input player.
    /// </summary>
    public void SetTargetLocalEuler(Vector3 euler)
    {
        _targetLocalRotation = Quaternion.Euler(ClampEuler(NormalizeEuler(euler)));
    }

    /// <summary>Tambah delta rotasi ke target saat ini (berguna untuk animasi prosedural/osilasi).</summary>
    public void AddDeltaEuler(Vector3 delta)
    {
        Vector3 current = NormalizeEuler(_targetLocalRotation.eulerAngles);
        SetTargetLocalEuler(current + delta);
    }

    public Vector2 GetLimit(Axis axis) => axis switch
    {
        Axis.X => limitX,
        Axis.Y => limitY,
        _ => limitZ,
    };

    private void Update()
    {
        if (rotationSpeed <= 0f)
        {
            transform.localRotation = _targetLocalRotation;
        }
        else
        {
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation, _targetLocalRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private Vector3 ClampEuler(Vector3 euler)
    {
        switch (jointType)
        {
            case JointType.Fixed:
                return Vector3.zero;

            case JointType.Hinge:
                return hingeAxis switch
                {
                    Axis.X => new Vector3(Mathf.Clamp(euler.x, limitX.x, limitX.y), 0f, 0f),
                    Axis.Z => new Vector3(0f, 0f, Mathf.Clamp(euler.z, limitZ.x, limitZ.y)),
                    _ => new Vector3(0f, Mathf.Clamp(euler.y, limitY.x, limitY.y), 0f),
                };

            default: // BallSocket
                return new Vector3(
                    Mathf.Clamp(euler.x, limitX.x, limitX.y),
                    Mathf.Clamp(euler.y, limitY.x, limitY.y),
                    Mathf.Clamp(euler.z, limitZ.x, limitZ.y));
        }
    }

    private static Vector3 NormalizeEuler(Vector3 euler)
    {
        euler.x = NormalizeAngle(euler.x);
        euler.y = NormalizeAngle(euler.y);
        euler.z = NormalizeAngle(euler.z);
        return euler;
    }

    private static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmo) return;

        Gizmos.color = gizmoColor;
        Quaternion parentRot = transform.parent ? transform.parent.rotation : Quaternion.identity;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, parentRot, Vector3.one);

        if (jointType == JointType.Hinge)
        {
            Vector2 limit = GetLimit(hingeAxis);
            Vector3 axisVec = hingeAxis switch
            {
                Axis.X => Vector3.right,
                Axis.Z => Vector3.forward,
                _ => Vector3.up,
            };
            DrawArc(axisVec, limit.x, limit.y);
        }
        else if (jointType == JointType.BallSocket)
        {
            DrawCone();
        }

        Gizmos.matrix = Matrix4x4.identity;
    }

    private void DrawArc(Vector3 axis, float min, float max)
    {
        const int segments = 16;
        Vector3 baseDir = axis == Vector3.up ? Vector3.forward : Vector3.up;
        Vector3 prev = Quaternion.AngleAxis(min, axis) * baseDir * gizmoSize;
        Gizmos.DrawLine(Vector3.zero, prev);
        for (int i = 1; i <= segments; i++)
        {
            float t = Mathf.Lerp(min, max, (float)i / segments);
            Vector3 next = Quaternion.AngleAxis(t, axis) * baseDir * gizmoSize;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
        Gizmos.DrawLine(Vector3.zero, prev);
    }

    private void DrawCone()
    {
        const int rays = 12;
        float maxSwing = Mathf.Max(
            Mathf.Abs(limitX.x), Mathf.Abs(limitX.y),
            Mathf.Abs(limitY.x), Mathf.Abs(limitY.y));

        for (int i = 0; i < rays; i++)
        {
            float angle = (360f / rays) * i;
            Vector3 swingDir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.up;
            Vector3 tip = Quaternion.AngleAxis(maxSwing, swingDir) * Vector3.forward * gizmoSize;
            Gizmos.DrawLine(Vector3.zero, tip);
        }
    }
#endif
}
