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
    public MagneticSurface positiveMag;
    public MagneticSurface negativeMag;

    [Header("Control Settings")]
    [SerializeField] private float rotationFactor;
    public float clingAngleClamp = 1.15f;

    [HideInInspector] public Vector2 cursorPosWS;
    [HideInInspector] public Vector2 mouseDelta;

    [SerializeField] private LayerMask clingLayerMask;
    //==============================================================================================================================

    private Vector3 startPos;
    private float angularVelocity;
    private Quaternion previousRotation; /// Used for calculating angular velocity.
    private Vector3 previousClingUp;    /// Used for determining input rotation direction.
    private Quaternion intermediateRot; /// Used for interpolating the rigidbody rotation of the player.
    [HideInInspector] public Vector2 clingSurfaceNormal; /// Stores normal information to constrain pivot rotation.
    MagneticSurface playerClingMag;     /// Which magnet player is using for cling.
    private Vector2 moveInput;
    private bool anchor;

    // Replace with state stuff later
    [HideInInspector] public WeldState weldState = WeldState.None;
    public enum WeldState
    {
        None,
        Welded,
        LaunchAim,
    }

    #endregion 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        startPos = transform.position;
        previousRotation = transform.rotation;
        previousClingUp = -negativeMag.transform.up;
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

        #region Weld To Surface
        // If Weld is inputted.
        if (anchor)
        {
            #region Cling

            Collider2D clingMagSurface = null; /// Surface of the magnet to cling to.
            bool magCharge = false;            /// The charge of the players magnet that is clinging.

            #region Get Cling Magnet Surface
            Collider2D[] positiveOverlap = Physics2D.OverlapCircleAll((Vector2)positiveMag.transform.position, .5f, clingLayerMask);
            Collider2D[] negativeOverlap = Physics2D.OverlapCircleAll((Vector2)negativeMag.transform.position, .5f, clingLayerMask);
            foreach (Collider2D col in positiveOverlap)
            {
                MagnetComponentBase curMag = col.gameObject.GetComponent<MagnetComponentBase>();
                // If can anchor to this magnet.
                if (curMag != null && curMag._magData.charge == -1)
                {
                    clingMagSurface = curMag.gameObject.GetComponent<Collider2D>();
                    magCharge = true; /// Refers to players clinging magnet charge.
                    break;
                }
            }

            if (clingMagSurface == null)
            {
                foreach (Collider2D col in negativeOverlap)
                {
                    MagnetComponentBase curMag = col.gameObject.GetComponent<MagnetComponentBase>();
                    // If can anchor to this magnet.
                    if (curMag != null && curMag._magData.charge == 1)
                    {
                        clingMagSurface = curMag.gameObject.GetComponent<Collider2D>();
                        magCharge = false; /// Refers to players clinging magnet charge.
                        break;
                    }
                }
            }
            #endregion

            if (clingMagSurface != null)
            {
                playerClingMag = (magCharge ? positiveMag : negativeMag);
                Vector3 nearestClingPoint = clingMagSurface.ClosestPoint(playerClingMag.transform.position);

                // Enable Hinge Joint and Set Rotation Constraints
                playerClingMag.GetComponent<HingeJoint2D>().enabled = true;
                
                
                // Apply Custom Force
                Vector3 clingAimDir = (nearestClingPoint - playerClingMag.transform.position).normalized;
                //Vector2 adjustedAimDir = (playerClingMag._magData.charge * clingMagSurface.gameObject.GetComponent<MagnetComponentBase>()._magData.charge == -1 ? clingAimDir : -clingAimDir);
                Quaternion targetClingAimQuat = Quaternion.LookRotation(Vector3.forward, clingAimDir);
                //rb.AddForceAtPosition(clingAimDir * 1000f, playerClingMag.transform.position);

                // Update surface normal. PUT THIS IN A DO ONCE!
                // Raycast to the point, get normal back.
                RaycastHit2D[] hits = Physics2D.RaycastAll(playerClingMag.transform.position, clingAimDir, 1f, clingLayerMask);
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider == clingMagSurface)
                    {
                        clingSurfaceNormal = hit.normal;
                    }
                }

                //clingSurfaceNormalQuat = Quaternion.LookRotation(Vector3.forward, -clingAimDir);

                // TO DO: Put this into state machine logic.
                ChangeWeldState(WeldState.Welded);
            }
            else
            {
                // TO DO: Put this into state machine logic.
                positiveMag.GetComponent<HingeJoint2D>().enabled = false;
                negativeMag.GetComponent<HingeJoint2D>().enabled = false;
                ChangeWeldState(WeldState.None);
            }
            #endregion
        }
        else
        {
            // TO DO: Put this into state machine logic.
            positiveMag.GetComponent<HingeJoint2D>().enabled = false;
            negativeMag.GetComponent<HingeJoint2D>().enabled = false;
            ChangeWeldState(WeldState.None);
        }
        #endregion

        #region Rotate Player
        // Get Aim Target Vector
        Vector3 aimDir = (pivotObj.transform.position - cursorObj.transform.position).normalized;
        Quaternion targetAimQuat = Quaternion.LookRotation(Vector3.forward, aimDir);

        if (weldState == WeldState.None)
        {
            cursorObj.GetComponent<SoftwareCursor>().parentForPos = gameObject;
            rotationFactor = 30f;

            // Manually Update Transforms
            transform.rotation = Quaternion.Slerp(transform.rotation, targetAimQuat, Time.deltaTime * rotationFactor);
            intermediateRot = transform.rotation;
        }
        else if (weldState == WeldState.Welded)
        {
            cursorObj.GetComponent<SoftwareCursor>().parentForPos = playerClingMag.gameObject;
            rotationFactor = 60f;

            Vector3 clingAimDir = (playerClingMag.transform.position - cursorObj.transform.position).normalized;
            Vector3 adjustedClingAimDir = (playerClingMag == positiveMag ? -1f : 1f) * ((playerClingMag.transform.position - cursorObj.transform.position).normalized);
            Quaternion clingTargetAimQuat = Quaternion.LookRotation(Vector3.forward, clingAimDir);
            //Debug.DrawLine(playerClingMag.transform.position, transform.position + (3f * (Vector3)clingSurfaceNormal), Color.black, .2f);
            //Debug.DrawLine(playerClingMag.transform.position, transform.position + (3f * clingAimDir), Color.hotPink, .2f);

            float angleFromSurfNormal = (float)System.Math.Round(Mathf.Atan2(clingSurfaceNormal.y, clingSurfaceNormal.x) - Mathf.Atan2(adjustedClingAimDir.y, adjustedClingAimDir.x), 2);

            // Is next rotation within clamped range?
            if (Mathf.Abs(angleFromSurfNormal) < clingAngleClamp && Mathf.Abs(angleFromSurfNormal) > -clingAngleClamp)
            {
                // Foddian Rigidbody rotation method
                intermediateRot = Quaternion.Slerp(intermediateRot, clingTargetAimQuat, Time.deltaTime * rotationFactor);
                //rb.MoveRotation(intermediateRot);
            }
                // I need to make the conditional above only update target rotation. Always update to last valid target rotation. This will account for fast mouse.
            rb.MoveRotation(intermediateRot);
        }
        #endregion

        #region Apply Magnetic Forces
        if (weldState == WeldState.None)
        {
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
                Debug.Log("Affect Field on Battery Negative: " + negativeFields[i]);
                Vector2 curForce = negativeFields[i].GetAppliedForce(negativeMag._magData, negativeMag.transform.position, negativeMag._fieldAttractDistance);
                Debug.Log("Negative Force: " + curForce);
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

            Debug.Log("Positive Fields: " + positiveFields.Count + " | " + "Negative Fields: " + negativeFields.Count);

            // Apply Forces

            float velocityMultiplier = 1f;// FunctionLibraryF.MapRangeClamped(0f, 10f, .8f, 1.15f, velocity);
            float angularMultiplier = 1f;//FunctionLibraryF.MapRangeClamped(0f, 25f, 1f, 1.25f, Mathf.Abs(angularVelocity));

            rb.AddForceAtPosition(combinedPositiveForces * velocityMultiplier * angularMultiplier, positiveMag.transform.position);
            Debug.DrawLine(positiveMag.transform.position, (Vector2)positiveMag.transform.position + combinedPositiveForces);

            rb.AddForceAtPosition(combinedNegativeForces * velocityMultiplier * angularMultiplier, negativeMag.transform.position);
            Debug.DrawLine(negativeMag.transform.position, (Vector2)negativeMag.transform.position + combinedNegativeForces);
            
        }
        #endregion
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

    public void ChangeWeldState(WeldState newState)
    {
        weldState = newState;
    }

    public void Restart()
    {
        transform.position = startPos;
        rb.linearVelocity = Vector3.zero;
    }
    #endregion
}
