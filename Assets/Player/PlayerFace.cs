using UnityEngine;
using FunctionLibrary;

public class PlayerFace : MonoBehaviour
{
    [SerializeField] private BatteryController batteryController;
    [SerializeField] private float maxRotationAmount = 45f;
    // Update is called once per frame
    void FixedUpdate()
    {
        if (Mathf.Abs(batteryController.GetRigidBody().angularVelocity) < 180f)
        {
            // Interp back to neutral rotation.
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.fixedDeltaTime * 1.5f);
        }
        else
        {
            // Lean into direction player is rotating.
            transform.rotation = transform.rotation * Quaternion.AngleAxis(batteryController.GetRigidBody().angularVelocity * .002f, Vector3.forward);
        }

        // Clamp rotation to be within angle bounds.
        float z = Mathf.DeltaAngle(0f, transform.rotation.eulerAngles.z);
        transform.localRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, Mathf.Clamp(z, -1f * maxRotationAmount, maxRotationAmount));
    }
}
