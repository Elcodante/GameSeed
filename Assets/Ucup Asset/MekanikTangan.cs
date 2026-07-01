using UnityEngine;

public class MekanikTangan : MonoBehaviour
{
    [Header("Masukkan Rigidbody Lengan (forearm.R) kesini")]
    public Rigidbody lenganRb;

    [Header("Masukkan Komponen Hinge Joint Jari kesini")]
    public HingeJoint engselTelunjuk;
    public HingeJoint engselJariTengah;

    void Start()
    {
        if (lenganRb != null)
        {
            lenganRb.useGravity = false;
            lenganRb.linearDamping = 5f;
        }
    }
}