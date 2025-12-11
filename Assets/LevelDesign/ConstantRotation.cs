using UnityEngine;

public class ConstantRotation : MonoBehaviour
{
    [SerializeField] private Vector3 localRotationPivot = Vector3.zero;
    [SerializeField] private float rotationSpeed = 20f;

    void FixedUpdate()
    {
        var dt = Time.fixedDeltaTime;

        transform.RotateAround(transform.TransformPoint(localRotationPivot), Vector3.forward, (rotationSpeed * dt));
    }
}
