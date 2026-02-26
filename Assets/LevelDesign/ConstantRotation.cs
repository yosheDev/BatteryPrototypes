using UnityEngine;

public class ConstantRotation : MonoBehaviour
{
    [SerializeField] private Vector3 localRotationPivotPoint = Vector3.zero;
    [SerializeField] private float rotationSpeed = 20f;

    void FixedUpdate()
    {
        var dt = Time.fixedDeltaTime;

        transform.RotateAround(transform.TransformPoint(localRotationPivotPoint), Vector3.forward, (rotationSpeed * dt));
    }
}
