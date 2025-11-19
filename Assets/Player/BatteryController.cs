using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using System.Linq;
using Magnet;

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
    #endregion 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        startPos = transform.position;
    }

    private void FixedUpdate()
    {
            // Get Aim Target Vector
            Vector3 aimDir = (transform.position - cursorObj.transform.position).normalized;
        Quaternion targetAimQuat = Quaternion.LookRotation(Vector3.forward, aimDir);

        // Apply rotation.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetAimQuat, Time.deltaTime * rotationFactor);

        #region Apply Magnetic Forces
        // Get Overlapped Magneting Fields From Magnet Components 
        /// Handle + and - sides seperately, as those will apply forces at different points of player collider.

        // [Handle Positive Magnet]
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
                combinedPositiveForces += Vector2.ClampMagnitude(Vector2.Lerp(curForce, (adjustedAimDir * curForce.magnitude), .5f), 100f);
                // Maybe make t a remap of velocity? More influence at higher velocity?
            }
        }

        // [Handle Negative Magnet]
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
                combinedNegativeForces += Vector2.ClampMagnitude(Vector2.Lerp(curForce, (adjustedAimDir * curForce.magnitude), .5f), 100f);

                // Maybe make t a remap of velocity? More influence at higher velocity?
            }
        }

        // Apply Forces
        rb.AddForceAtPosition(combinedPositiveForces, positiveMag.transform.position);
        Debug.DrawLine(positiveMag.transform.position, (Vector2)positiveMag.transform.position + combinedPositiveForces);
        rb.AddForceAtPosition(combinedNegativeForces, negativeMag.transform.position);
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
