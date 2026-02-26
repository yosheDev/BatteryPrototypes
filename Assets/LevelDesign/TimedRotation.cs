using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TimedRotation : MonoBehaviour
{
    [SerializeField] private Vector3 localRotationPivotPoint = Vector3.zero;
    [SerializeField] private float rotationSpeed = 20f;

    [SerializeField] private float rotationIncrement = 90;    /// In degrees
    [SerializeField] private float reachedDelay = 2f;

    private Coroutine _rotationRoutine;
    private Quaternion _targetRot;

    private void Start()
    {
        _targetRot = transform.rotation * Quaternion.Euler(new Vector3(0f, 0f, rotationIncrement));
        _rotationRoutine = StartCoroutine(RotateByIncrement(rotationIncrement, reachedDelay));
    }

    private IEnumerator RotateByIncrement(float increment, float delay)
    {
        // Begin Rotation
        float rotatedDegrees = 0;
        while (rotatedDegrees <= Mathf.Abs(increment))
        {
            yield return new WaitForFixedUpdate();      /// Sync the coroutine with the Physics updates.
            transform.RotateAround(transform.TransformPoint(localRotationPivotPoint), Vector3.forward, (rotationSpeed * Time.fixedDeltaTime));
            rotatedDegrees += (rotationSpeed * Time.fixedDeltaTime);
        }

        // Finished rotation.
        yield return new WaitForSeconds(delay);
        _rotationRoutine = StartCoroutine(RotateByIncrement(rotationIncrement, reachedDelay));
    }
}
