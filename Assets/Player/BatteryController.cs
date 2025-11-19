using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using System.Linq;
using Magnet;
using FunctionLibrary;

public class BatteryController : MonoBehaviour
{
    #region Properties
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Camera mainCam;
    [SerializeField] private GameObject cursorObj;
    [SerializeField] private MagneticSurface positiveMag;
    [SerializeField] private MagneticSurface negativeMag;

    [Header("Control Settings")]
    [SerializeField] private float rotationFactor;

    [HideInInspector] public Vector2 cursorPosWS;
    [HideInInspector] public Vector2 mouseDelta;
    //==============================================================================================================================

    private Vector3 startPos;
    private float angularVelocity;
    private Quaternion previousRotation;

    #endregion 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        startPos = transform.position;
        previousRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        float velocity = (Mathf.Abs(rb.linearVelocity.x) + Mathf.Abs(rb.linearVelocity.y)) * 0.5f;
        //Debug.Log("Velocity: " + velocity);

        Quaternion currentRotation = transform.rotation;
        Vector3 rotationChange = currentRotation.eulerAngles - previousRotation.eulerAngles;
        angularVelocity = (rotationChange.z + 180f) % 360f - 180f;
        previousRotation = currentRotation;

        // Get Aim Target Vector
        Vector3 aimDir = (transform.position - cursorObj.transform.position).normalized;
        Quaternion targetAimQuat = Quaternion.LookRotation(Vector3.forward, aimDir);

        // Apply rotation.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetAimQuat, Time.deltaTime * rotationFactor);

        #region Apply Magnetic Forces
        // Get Overlapped Magneting Fields From Magnet Components 
        /// Handle + and - sides seperately, as those will apply forces at different points of player collider.

        #region Combine Positive Magnet Forces
        Vector2 combinedPositiveForces = Vector2.zero;
        Vector2 positiveMagDir = (positiveMag.transform.up).normalized;

        List<MagnetComponentBase> positiveFields = positiveMag.affectFields.ToList();
        for (int i = 0; i < positiveFields.Count; i++)
        {
            Vector2 curForce = positiveFields[i].GetAppliedForce(positiveMag._magData, positiveMag.transform.position, positiveMag._fieldAttractDistance);

            // Prevent NaN
            if (float.IsNaN(curForce.x))
            {
                curForce = Vector2.zero;
            }

            Vector2 adjustedAimDir = (positiveFields[i]._magData.charge * positiveMag._magData.charge == -1 ? positiveMagDir : -positiveMagDir);
            // Will not be accounted for if pole is facing opposite direction (for game feel.)
            if (Vector2.Dot(adjustedAimDir, curForce.normalized) > 0.4f)
            {
                float aimDirInfluence = FunctionLibraryF.MapRangeClamped(0f, 10f, .6f, .3f, velocity);
                combinedPositiveForces += Vector2.ClampMagnitude(Vector2.Lerp(curForce, (adjustedAimDir * curForce.magnitude), aimDirInfluence), 100f);
            }
        }
        #endregion

        #region Combine Negative Magnet Forces
        Vector2 combinedNegativeForces = Vector2.zero;
        Vector2 negativeMagDir = (negativeMag.transform.up).normalized;

        List<MagnetComponentBase> negativeFields = negativeMag.affectFields.ToList();
        for (int i = 0; i < negativeFields.Count; i++)
        {
            Vector2 curForce = negativeFields[i].GetAppliedForce(negativeMag._magData, negativeMag.transform.position, negativeMag._fieldAttractDistance);

            // Prevent NaN
            if (float.IsNaN(curForce.x))
            {
                curForce = Vector2.zero;
            }

            Vector2 adjustedAimDir = (negativeFields[i]._magData.charge * negativeMag._magData.charge == -1 ? negativeMagDir : -negativeMagDir);
            // Will not be accounted for if pole is facing opposite direction (for game feel.)
            if (Vector2.Dot(adjustedAimDir, curForce.normalized) > 0.4f)
            {
                float aimDirInfluence = FunctionLibraryF.MapRangeClamped(0f, 10f, .6f, .3f, velocity);
                combinedNegativeForces += Vector2.ClampMagnitude(Vector2.Lerp(curForce, (adjustedAimDir * curForce.magnitude), aimDirInfluence), 100f);
            }
        }
        #endregion

        // Apply Forces

        float velocityMultiplier = 1f;// FunctionLibraryF.MapRangeClamped(0f, 10f, .8f, 1.15f, velocity);
        float angularMultiplier = FunctionLibraryF.MapRangeClamped(0f, 25f, 1f, 1.25f, Mathf.Abs(angularVelocity));

        rb.AddForceAtPosition(combinedPositiveForces * velocityMultiplier * angularMultiplier, positiveMag.transform.position);
        Debug.DrawLine(positiveMag.transform.position, (Vector2)positiveMag.transform.position + combinedPositiveForces);

        rb.AddForceAtPosition(combinedNegativeForces * velocityMultiplier * angularMultiplier, negativeMag.transform.position);
        Debug.DrawLine(negativeMag.transform.position, (Vector2)negativeMag.transform.position + combinedNegativeForces);
        #endregion
    }

    #region Input Actions
    public void UpdateMouseDelta(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>().magnitude < 50f ? (context.ReadValue<Vector2>()) : mouseDelta; /// Magnitude check protects against mouse connectivity errors.
    }

    // Currently unused.
    public void Tilt(InputAction.CallbackContext context)
    {
        if (playerInput.currentControlScheme.Equals("Gamepad"))
        {

        }
        else
        {
            //cursorPosWS = mainCam.ScreenToWorldPoint(context.ReadValue<Vector2>());
            
        }
    }

    public void Restart()
    {
        transform.position = startPos;
        rb.linearVelocity = Vector3.zero;
    }
    #endregion
}
