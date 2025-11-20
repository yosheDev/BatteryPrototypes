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
    [SerializeField] private BoxCollider2D surfaceCol;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Camera mainCam;
    [SerializeField] private GameObject cursorObj;
    [SerializeField] private GameObject pivotObj;
    [SerializeField] private MagneticSurface positiveMag;
    [SerializeField] private MagneticSurface negativeMag;

    [Header("Control Settings")]
    [SerializeField] private float rotationFactor;

    [HideInInspector] public Vector2 cursorPosWS;
    [HideInInspector] public Vector2 mouseDelta;

    [SerializeField] private LayerMask clingLayerMask;
    //==============================================================================================================================

    private Vector3 startPos;
    private float angularVelocity;
    private Quaternion previousRotation; /// Used for calculating angular velocity.
    private Quaternion intermediateRot; /// Used for interpolating the rigidbody rotation of the player.
    private Vector2 moveInput;
    private bool anchor;

    // Replace with state stuff later
    private bool isClinging = false;

    #endregion 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        startPos = transform.position;
        previousRotation = transform.rotation;
        intermediateRot = transform.rotation;
    }

    private void FixedUpdate()
    {
        float velocity = (Mathf.Abs(rb.linearVelocity.x) + Mathf.Abs(rb.linearVelocity.y)) * 0.5f;
        //Debug.Log("Velocity: " + velocity);

        Quaternion currentRotation = transform.rotation;
        Vector3 rotationChange = currentRotation.eulerAngles - previousRotation.eulerAngles;
        angularVelocity = (rotationChange.z + 180f) % 360f - 180f;
        previousRotation = currentRotation;

        #region Point To Software Cursor

        if (anchor)
        {
            #region Cling

            Collider2D aimMag = null;
            bool magCharge = false;

            Collider2D[] positiveOverlap = Physics2D.OverlapCircleAll((Vector2)positiveMag.transform.position, .5f, clingLayerMask);
            Collider2D[] negativeOverlap = Physics2D.OverlapCircleAll((Vector2)negativeMag.transform.position, .5f, clingLayerMask);
            foreach (Collider2D col in positiveOverlap)
            {
                MagnetComponentBase curMag = col.gameObject.GetComponent<MagnetComponentBase>();
                // If can anchor to this magnet.
                if (curMag != null && curMag._magData.charge == -1)
                {
                    // Unneeded right now, as players magnetic fields are excluded in LayerMask.
                    //if (curMag is MagneticSurface)
                    //{
                    //    if (((MagneticSurface)curMag).fieldCol.gameObject.GetComponent<MagneticTriggers>().isOnPlayer == false)
                    //    {

                    //    }
                    //}
                    Debug.DrawLine(positiveMag.transform.position, positiveMag.transform.position + new Vector3(1f, 1f, 0f), Color.yellow, 2f);
                    aimMag = curMag.gameObject.GetComponent<Collider2D>();
                    magCharge = true; /// Refers to players clinging magnet charge.
                    break;
                }
            }

            if (aimMag == null)
            {
                foreach (Collider2D col in negativeOverlap)
                {
                    MagnetComponentBase curMag = col.gameObject.GetComponent<MagnetComponentBase>();
                    // If can anchor to this magnet.
                    if (curMag != null && curMag._magData.charge == 1)
                    {
                        Debug.DrawLine(negativeMag.transform.position, negativeMag.transform.position + new Vector3(1f, 1f, 0f), Color.yellow, 2f);
                        aimMag = curMag.gameObject.GetComponent<Collider2D>();
                        magCharge = false; /// Refers to players clinging magnet charge.
                        break;
                    }
                }
            }
            
            if (aimMag != null)
            {
                MagneticSurface playerClingMag = (magCharge ? positiveMag : negativeMag);
                Vector3 nearestClingPoint = aimMag.ClosestPoint(playerClingMag.transform.position);

                // Get Aim Target Vector
                Vector3 clingAimDir = (nearestClingPoint - playerClingMag.transform.position).normalized;
                //Vector2 adjustedAimDir = (playerClingMag._magData.charge * aimMag.gameObject.GetComponent<MagnetComponentBase>()._magData.charge == -1 ? aimDir : -aimDir);

                Debug.DrawLine(playerClingMag.transform.position, (Vector2)playerClingMag.transform.position + (Vector2)(clingAimDir * 3f), Color.red, 1f);

                Quaternion targetClingAimQuat = Quaternion.LookRotation(Vector3.forward, clingAimDir);

                // Apply rotation.
                //transform.rotation = Quaternion.Slerp(transform.rotation, targetAimQuat, Time.deltaTime * rotationFactor);

                isClinging = true;

                rb.AddForceAtPosition(clingAimDir * 1000f, playerClingMag.transform.position);
                //Debug.Log("Adding force for cling");
            }
            else
            {
                isClinging = false;
            }
            #endregion
        }
        else
        {
            isClinging = false;
        }

        // Get Aim Target Vector
        Vector3 aimDir = (pivotObj.transform.position - cursorObj.transform.position).normalized;
        Quaternion targetAimQuat = Quaternion.LookRotation(Vector3.forward, aimDir);

        if (!isClinging)
        {
            // Manually Update Transforms
            transform.rotation = Quaternion.Slerp(transform.rotation, targetAimQuat, Time.deltaTime * rotationFactor);
            intermediateRot = transform.rotation;
        }
        else
        {
            // Foddian Rigidbody rotation method
            intermediateRot = Quaternion.Slerp(intermediateRot, targetAimQuat, Time.deltaTime * rotationFactor);
            rb.MoveRotation(intermediateRot);
        }

        // Trying to be able to customize the pivot point of rotation based on a transform location.

        //float angleBetweenDirs = Vector2.Angle(transform.up, aimDir);
        //float angleBetweenDirs = Mathf.Atan2(aimDir.y, aimDir.x) - Mathf.Atan2(transform.up.y, transform.up.x);
        //atan2(vector2.y, vector2.x) - atan2(vector1.y, vector1.x)
        //Transform destination = transform;

        // Trying to let me rotate around a speficic pivot point.
        // destination.RotateAround(pivotObj.transform.position, Vector3.forward, angleBetweenDirs);
        //Quaternion targetAimQuat = transform.rotation;
        #endregion

        if (!isClinging)
        {
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
                    combinedPositiveForces += Vector2.ClampMagnitude((curForce.magnitude * Vector2.Lerp(curForce, (adjustedAimDir * curForce.magnitude), aimDirInfluence).normalized), 100f);
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
                    combinedNegativeForces += Vector2.ClampMagnitude((curForce.magnitude * Vector2.Lerp(curForce, (adjustedAimDir * curForce.magnitude), aimDirInfluence).normalized), 100f);
                }
            }
            #endregion

            // Apply Forces

            float velocityMultiplier = 1f;// FunctionLibraryF.MapRangeClamped(0f, 10f, .8f, 1.15f, velocity);
            float angularMultiplier = 1f;//FunctionLibraryF.MapRangeClamped(0f, 25f, 1f, 1.25f, Mathf.Abs(angularVelocity));

            rb.AddForceAtPosition(combinedPositiveForces * velocityMultiplier * angularMultiplier, positiveMag.transform.position);
            Debug.DrawLine(positiveMag.transform.position, (Vector2)positiveMag.transform.position + combinedPositiveForces);

            rb.AddForceAtPosition(combinedNegativeForces * velocityMultiplier * angularMultiplier, negativeMag.transform.position);
            Debug.DrawLine(negativeMag.transform.position, (Vector2)negativeMag.transform.position + combinedNegativeForces);
            #endregion
        }

    }

    #region Input Actions
    public void UpdateMouseDelta(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>().magnitude < 50f ? (context.ReadValue<Vector2>()) : mouseDelta; /// Magnitude check protects against mouse connectivity errors.
    }

    // Currently unused.
    public void Move(InputAction.CallbackContext context)
    {
        if (playerInput.currentControlScheme.Equals("Gamepad"))
        {

        }
        else
        {
            moveInput = context.ReadValue<Vector2>();
        }
    }

    public void Anchor(InputAction.CallbackContext context)
    {
        if (playerInput.currentControlScheme.Equals("Gamepad"))
        {
            
        }
        else
        {
            anchor = (context.ReadValue<float>() == 1 ? true : false);
        }
    }

    public void Restart()
    {
        transform.position = startPos;
        rb.linearVelocity = Vector3.zero;
    }
    #endregion
}
