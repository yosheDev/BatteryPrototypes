using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using System.Linq;
using Magnet;
using FunctionLibrary;
using UnityEngine.Rendering;

public class BatteryController : MonoBehaviour
{
    #region Properties
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D surfaceCol;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Camera mainCam;
    [SerializeField] private GameObject cursorObj;
    private SoftwareCursor softwareCursor;
    [SerializeField] private GameObject scalePivot;
    [SerializeField] private GameObject weldBlob;
    public MagneticSurface positiveMag;
    public MagneticSurface negativeMag;

    [Header("Control Settings")]
    [SerializeField] private float rotationFactor;
    public float weldAngleClamp = 1.15f;
    [SerializeField] private float launchMaxExponent = 1.13f;
    [SerializeField] private float launchBaseConstant = 500f;

    [HideInInspector] public Vector2 cursorPosWS;
    [HideInInspector] public Vector2 mouseDelta;

    [SerializeField] private LayerMask weldLayerMask;
    //==============================================================================================================================

    private Vector3 startPos;
    private float angularVelocity;
    private Quaternion previousRotation; /// Used for calculating angular velocity.
    private Vector3 previousWeldUp;    /// Used for determining input rotation direction.
    private Quaternion intermediateRot; /// Used for interpolating the rigidbody rotation of the player.
    [HideInInspector] public Vector2 weldSurfaceNormal; /// Stores normal information to constrain pivot rotation.
    MagneticSurface playerWeldMag;     /// Which magnet player is using for cling.
    Vector3 adjustedWeldAimDir;
    private Vector2 moveInput;
    private bool weldInput;
    private bool launchInput;
    private InputAction launchAction;

    // Replace with state stuff later
    [HideInInspector] public WeldState weldState = WeldState.None;
    private bool lockWeldState = false;
    public enum WeldState
    {
        None,
        Welded,
        LaunchAim,
    }

    #endregion 

    void Start()
    {
        // Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        softwareCursor = cursorObj.GetComponent<SoftwareCursor>();

        // Initialize
        startPos = transform.position;
        previousRotation = transform.rotation;
        previousWeldUp = -negativeMag.transform.up;
        intermediateRot = transform.rotation;
    }

    private void FixedUpdate()
    {
        #region Initialize Common Variables
        // Velocity of player.
        float velocity = (Mathf.Abs(rb.linearVelocity.x) + Mathf.Abs(rb.linearVelocity.y)) * 0.5f;
        ///Debug.Log("Velocity: " + velocity);

        // Angular velocity of player.
        Quaternion currentRotation = transform.rotation;
        Vector3 rotationChange = currentRotation.eulerAngles - previousRotation.eulerAngles;
        angularVelocity = (rotationChange.z + 180f) % 360f - 180f;
        previousRotation = currentRotation;

        // Get Aim Target Vector and Quat. Used for rotating player.
        Vector3 aimDir = (transform.position - cursorObj.transform.position).normalized;
        Quaternion targetAimQuat = Quaternion.LookRotation(Vector3.forward, aimDir);
        #endregion

        #region Weld To Surface / Handle Weld Input
        // If Weld is inputted.
        if (weldInput)
        {
            #region Weld
            Collider2D weldMagSurface = null; /// Surface of the magnet to cling to.
            bool magCharge = false;            /// The charge of the players magnet that is clinging.

            #region Get Cling Magnet Surface
            Collider2D[] positiveOverlap = Physics2D.OverlapCircleAll((Vector2)positiveMag.transform.position, .5f, weldLayerMask);
            Collider2D[] negativeOverlap = Physics2D.OverlapCircleAll((Vector2)negativeMag.transform.position, .5f, weldLayerMask);
            foreach (Collider2D col in positiveOverlap)
            {
                MagnetComponentBase curMag = col.gameObject.GetComponent<MagnetComponentBase>();
                // If can weld to this magnet.
                if (curMag != null && curMag._magData.charge == -1)
                {
                    weldMagSurface = curMag.gameObject.GetComponent<Collider2D>();
                    magCharge = true; /// Refers to players clinging magnet charge.
                    break;
                }
            }

            if (weldMagSurface == null)
            {
                foreach (Collider2D col in negativeOverlap)
                {
                    MagnetComponentBase curMag = col.gameObject.GetComponent<MagnetComponentBase>();
                    // If can weld to this magnet.
                    if (curMag != null && curMag._magData.charge == 1)
                    {
                        weldMagSurface = curMag.gameObject.GetComponent<Collider2D>();
                        magCharge = false; /// Refers to players clinging magnet charge.
                        break;
                    }
                }
            }
            #endregion

            if (weldMagSurface != null)
            {
                if (weldState != WeldState.Welded && weldState != WeldState.LaunchAim)
                {
                    playerWeldMag = (magCharge ? positiveMag : negativeMag);
                    Vector3 nearestWeldPoint = weldMagSurface.ClosestPoint(playerWeldMag.transform.position);

                    // Apply Custom Force
                    Vector3 weldAimDir = (nearestWeldPoint - playerWeldMag.transform.position).normalized;
                    //Vector2 adjustedAimDir = (playerWeldMag._magData.charge * weldMagSurface.gameObject.GetComponent<MagnetComponentBase>()._magData.charge == -1 ? weldAimDir : -weldAimDir);
                    Quaternion targetWeldAimQuat = Quaternion.LookRotation(Vector3.forward, weldAimDir);
                    //rb.AddForceAtPosition(weldAimDir * 1000f, playerWeldMag.transform.position);

                    // Raycast to the point, get normal back.
                    RaycastHit2D[] hits = Physics2D.RaycastAll(playerWeldMag.transform.position, weldAimDir, 1f, weldLayerMask);
                    foreach (RaycastHit2D hit in hits)
                    {
                        if (hit.collider == weldMagSurface)
                        {
                            weldSurfaceNormal = hit.normal;
                        }
                    }
                    ChangeWeldState(WeldState.Welded);
                }
            }
            else
            {
                ChangeWeldState(WeldState.None);
            }
            #endregion
        }
        else
        {
            ChangeWeldState(WeldState.None);
        }
        #endregion

        #region Weld State Update Functionality
        if (weldState == WeldState.None)
        {
            // Manually Update Transforms
            transform.rotation = Quaternion.Slerp(transform.rotation, targetAimQuat, Time.deltaTime * rotationFactor);
            intermediateRot = transform.rotation;
        }
        else if (weldState == WeldState.Welded)
        {
            Vector3 weldAimDir = (playerWeldMag.transform.position - cursorObj.transform.position).normalized;
            adjustedWeldAimDir = (playerWeldMag == positiveMag ? -1f : 1f) * ((playerWeldMag.transform.position - cursorObj.transform.position).normalized);
            Quaternion weldTargetAimQuat = Quaternion.LookRotation(Vector3.forward, weldAimDir);
            //Debug.DrawLine(playerWeldMag.transform.position, transform.position + (3f * (Vector3)weldSurfaceNormal), Color.black, .2f);
            //Debug.DrawLine(playerWeldMag.transform.position, transform.position + (3f * weldAimDir), Color.hotPink, .2f);

            float angleFromSurfNormal = (float)System.Math.Round(Mathf.Atan2(weldSurfaceNormal.y, weldSurfaceNormal.x) - Mathf.Atan2(adjustedWeldAimDir.y, adjustedWeldAimDir.x), 2);

            // Is next rotation within clamped range?
            if (Mathf.Abs(angleFromSurfNormal) < weldAngleClamp && Mathf.Abs(angleFromSurfNormal) > -weldAngleClamp)
            {
                // Foddian Rigidbody rotation method
                intermediateRot = Quaternion.Slerp(intermediateRot, weldTargetAimQuat, Time.deltaTime * rotationFactor);
                //rb.MoveRotation(intermediateRot);
            }

            rb.MoveRotation(intermediateRot);
        }
        else if (weldState == WeldState.LaunchAim)
        {
            scalePivot.transform.localScale = new Vector3(FunctionLibraryF.MapRangeClamped(0.4f, 1f, 1f, 1.25f, softwareCursor.GetLaunchAlpha()), Mathf.Lerp(1f, .5f, softwareCursor.GetLaunchAlpha()), 1f);
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

    public void Weld(InputAction.CallbackContext context)
    {
        if (playerInput.currentControlScheme.Equals("Gamepad"))
        {
            
        }
        else
        {
            weldInput = (context.ReadValue<float>() == 1 ? true : false);
        }
    }

    public void ChangeWeldState(WeldState newState)
    {
        //Debug.Log("Changing Weld State to " + newState);

        // Prevents state from changing.
        if (lockWeldState)
        {
            return;
        }
        // When leaving Launch Aim, unparent player from the squash/stretch pivot.
        if (weldState == WeldState.LaunchAim)
        {
            scalePivot.transform.localScale = Vector3.one;
            gameObject.transform.parent = scalePivot.transform.parent;
        }

        weldState = newState;
        switch(weldState)
        {
            case WeldState.None:
                cursorObj.GetComponent<SoftwareCursor>().parentForPos = gameObject;
                rotationFactor = 30f;
                positiveMag.GetComponent<HingeJoint2D>().enabled = false;
                negativeMag.GetComponent<HingeJoint2D>().enabled = false;
                weldBlob.SetActive(false);
                break;
            case WeldState.Welded:
                cursorObj.GetComponent<SoftwareCursor>().parentForPos = playerWeldMag.gameObject;
                rotationFactor = 60f;
                playerWeldMag.GetComponent<HingeJoint2D>().enabled = true;
                weldBlob.transform.position = playerWeldMag.transform.position + ((Vector3)weldSurfaceNormal * -.1f);
                weldBlob.transform.rotation = Quaternion.LookRotation(weldBlob.transform.forward, weldSurfaceNormal);
                weldBlob.SetActive(true);
                break;
            case WeldState.LaunchAim:
                rotationFactor = 0f;
                
                // Parent player to squash stretch pivot.
                scalePivot.transform.position = playerWeldMag.transform.position;
                scalePivot.transform.rotation = playerWeldMag.transform.rotation * Quaternion.Euler(0f, 0f, 180f);

                gameObject.transform.parent = scalePivot.transform;
                break;
            default:
                rotationFactor = 30f;
                break;
        }
    }

    public IEnumerator LockWeldState(float duration)
    {
        lockWeldState = true;
        yield return new WaitForSeconds(duration);
        lockWeldState = false;
        yield break;
    }
    public void InitiateLaunch(InputAction.CallbackContext context)
    {
        if (context.canceled && weldState == WeldState.LaunchAim)
        {
            LaunchFromWeld();
        }
        else
        {
            if (!context.started && !context.canceled)
            {
                if (playerInput.currentControlScheme.Equals("Gamepad"))
                {

                }
                else
                {
                    if (weldInput)
                    {
                        Debug.Log("Launch Aim");
                        launchInput = (context.ReadValue<float>() == 1 ? true : false);
                        ChangeWeldState(WeldState.LaunchAim);
                    }
                    else
                    {
                        launchInput = false;
                    }
                }
            }
        } 
    }

    public void LaunchFromWeld()
    {
        ChangeWeldState(WeldState.None);
        //weldInput = false; /// Flush input (uncommenting means players will need to repress space to weld to another surface after launch.)
        StartCoroutine(LockWeldState(.2f));

        float launchForce = Mathf.Pow(launchBaseConstant, FunctionLibraryF.MapRangeClamped(0f, 1f, 1f, launchMaxExponent, softwareCursor.GetLaunchAlpha()));
        rb.AddForceAtPosition(-playerWeldMag.transform.up * launchForce, playerWeldMag.transform.position);
    }

    public void CancelLaunch(InputAction.CallbackContext context)
    {
        Debug.Log("Launch is cancelled.");
        ChangeWeldState(WeldState.Welded);
    }
    public void Restart()
    {
        transform.position = startPos;
        rb.linearVelocity = Vector3.zero;
    }
    #endregion
}
