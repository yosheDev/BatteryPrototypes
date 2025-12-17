using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using System.Linq;
using Magnet;
using FunctionLibrary;
using UnityEngine.Animations;

public class BatteryController : MonoBehaviour
{
    #region Properties
    [Header("References")]
    [SerializeField] private GameObject scalePivot;
    [SerializeField] private GameObject weldBlob;
    [SerializeField] private GameObject spawnCage;
    private Camera mainCam;
    private Rigidbody2D rb;
    private Collider2D surfaceCol;
    private PlayerInput playerInput;
    private PlayerNeutralDetector neutralDetector;  
    private GameObject cursorObj;
    private SoftwareCursor softwareCursor;
    public MagneticSurface positiveMag;
    public MagneticSurface negativeMag;

    [Header("Control Settings")]
    [SerializeField] private float rotationFactor;

    // Weld
    public float weldAngleClamp = 75f;
    [SerializeField] private LayerMask weldLayerMask;

    // Launch
    [SerializeField] private float launchMaxExponent = 1.13f;
    [SerializeField] private float launchBaseConstant = 500f;

    //==============================================================================================================================
    #region Private
    // Surface Parent
    private GameObject surfaceParent;
    private Quaternion parentLastRot;
    private Vector3 parentLastPos;
    private Vector3 surfaceParentVelocity;

    // Cursor
    [HideInInspector] public Vector2 cursorPosWS;
    [HideInInspector] public Vector2 mouseDelta;

    // Welding
    public enum WeldState
    {
        None,
        Welded,
        LaunchAim,
    }

    [HideInInspector] public Vector2 weldSurfaceNormal; /// Stores normal information to constrain pivot rotation.
    [HideInInspector] public MagneticSurface playerWeldMag;     /// Which magnet player is using for cling.
    private Collider2D weldedSurface;                           /// Surface player is welded to. Used mostly to get the gameObject.
    private Vector3 adjustedWeldAimDir;
    [HideInInspector] public WeldState weldState = WeldState.None;
    private bool lockWeldState = false;

    // Intermediary
    [HideInInspector] public float velocity;
    [HideInInspector] public float angularVelocity;
    private Quaternion previousRotation; /// Used for calculating angular velocity.
    private Vector3 previousWeldUp;    /// Used for determining input rotation direction.
    private Quaternion intermediateRot; /// Used for interpolating the rigidbody rotation of the player.

    // Input System
    private Vector2 moveInput;
    private bool weldInput;
    private bool launchInput;
    private InputAction launchAction;

    private Vector3 startPos; /// Used for current restart. REMOVE THIS LATER when restart is tied into AreaManager/PlayerStart stuff.
    #endregion
    // =============================================================================================================================
    #endregion

    void Start()
    {
        #region Initialize References
        // Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cursorObj = transform.parent.gameObject.GetComponentInChildren<SoftwareCursor>().gameObject;
        softwareCursor = cursorObj.GetComponent<SoftwareCursor>();

        // Other References
        mainCam = Camera.main;
        surfaceCol = GetComponent<CapsuleCollider2D>();
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
        neutralDetector = GetComponentInChildren<PlayerNeutralDetector>();
        #endregion

        #region Initialize Variables
        // Initialize
        startPos = transform.position;
        previousRotation = transform.rotation;
        previousWeldUp = -negativeMag.transform.up;
        intermediateRot = transform.rotation;
        #endregion
    }

    private void FixedUpdate()
    {
        var dt = Time.fixedDeltaTime;

        #region Inherit Surface Parent Delta

        if (surfaceParent != null)
        {
            // TO DO: Will need to ensure I have the parents proper rotation pivot. Make interface to retrieve it. If not exist, use surfaceParent.transform.position.
            Vector3 parentPivot = surfaceParent.transform.position;

            Vector3 surfaceParentPosDelta = surfaceParent.transform.position - parentLastPos;
            surfaceParentVelocity = surfaceParentPosDelta / dt;

            float rotAngle = Quaternion.Angle(surfaceParent.transform.rotation, parentLastRot);
            rotAngle = -1f * Vector3.SignedAngle((surfaceParent.transform.rotation * Vector3.up), (parentLastRot * Vector3.up), Vector3.forward);

            // Rotate and Move Player
            if (gameObject.transform.parent == scalePivot.transform)
            {
                scalePivot.transform.position += surfaceParentPosDelta;
                scalePivot.transform.RotateAround(parentPivot, Vector3.forward, rotAngle);
            }
            else
            {
                transform.position += surfaceParentPosDelta;
                transform.RotateAround(parentPivot, Vector3.forward, rotAngle);
            }

            parentLastPos = surfaceParent.transform.position;
            parentLastRot = surfaceParent.transform.rotation;
        }

        #endregion

        #region Initialize Common Variables
        // Velocity of player.
        velocity = (Mathf.Abs(rb.linearVelocity.x) + Mathf.Abs(rb.linearVelocity.y)) * 0.5f;
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

        // If not in a room transition.
        if (AreaManager.instance.IsTransitionState(AreaManager.AreaTransitionState.None))
        {
            #region Weld To Surface / Handle Weld Input
            // If Weld is inputted.
            if (weldInput)
            {
                #region Weld
                Collider2D evalWeldSurface = null; /// Surface of the magnet to cling to.
                bool magCharge = false;            /// The charge of the players magnet that is clinging.

                #region Get Cling Magnet Surface
                Collider2D[] positiveOverlap = Physics2D.OverlapCircleAll((Vector2)positiveMag.transform.position, .3f, weldLayerMask);
                Collider2D[] negativeOverlap = Physics2D.OverlapCircleAll((Vector2)negativeMag.transform.position, .3f, weldLayerMask);

                // Positive
                if (evalWeldSurface == null)
                {
                    foreach (Collider2D col in positiveOverlap)
                    {
                        MagnetComponentBase curMag = col.gameObject.GetComponent<MagnetComponentBase>();
                        // If can weld to this magnet.
                        if (curMag != null && curMag._magData.charge == -1)
                        {
                            RaycastHit2D[] hits = Physics2D.LinecastAll(positiveMag.transform.position + (-positiveMag.transform.up * .2f), col.ClosestPoint(positiveMag.transform.position), Physics2D.DefaultRaycastLayers);
                            bool isOccluded = false;
                            foreach (RaycastHit2D hit in hits)
                            {
                                if (hit.collider != null && hit.collider.gameObject.GetComponent<FieldOccluder>())
                                {
                                    isOccluded = true;
                                    break;
                                }
                            }

                            if (!isOccluded)
                            {
                                evalWeldSurface = curMag.gameObject.GetComponent<Collider2D>();
                                magCharge = true; /// Refers to players clinging magnet charge.
                                break;
                            }
                        }
                    }
                }

                // Negative
                if (evalWeldSurface == null)
                {
                    foreach (Collider2D col in negativeOverlap)
                    {
                        MagnetComponentBase curMag = col.gameObject.GetComponent<MagnetComponentBase>();
                        // If can weld to this magnet.
                        if (curMag != null && curMag._magData.charge == 1)
                        {
                            RaycastHit2D[] hits = Physics2D.LinecastAll(negativeMag.transform.position + (-negativeMag.transform.up * .2f), col.ClosestPoint(negativeMag.transform.position), Physics2D.DefaultRaycastLayers);
                            bool isOccluded = false;
                            foreach (RaycastHit2D hit in hits)
                            {
                                if (hit.collider != null && hit.collider.gameObject.GetComponent<FieldOccluder>())
                                {
                                    isOccluded = true;
                                    break;
                                }
                            }

                            if (!isOccluded)
                            {
                                evalWeldSurface = curMag.gameObject.GetComponent<Collider2D>();
                                magCharge = false; /// Refers to players clinging magnet charge.
                                break;
                            }
                        }
                    }
                }
                #endregion

                if (evalWeldSurface != null)
                {
                    // Update Weld Vectors to match.
                    /// (Somewhat expensive, but can't be avoided as this needs done every frame to account for rotating/translating parent surfaces.)
                    weldedSurface = evalWeldSurface;
                    playerWeldMag = (magCharge ? positiveMag : negativeMag);
                    Vector3 nearestWeldPoint = weldedSurface.ClosestPoint(playerWeldMag.transform.position);
                    // Apply Custom Force
                    Vector2 weldAimDir = ((Vector2)nearestWeldPoint - (Vector2)(playerWeldMag.transform.position + (-playerWeldMag.transform.up * .5f))).normalized;
                    //Vector2 adjustedWeldAimDir = (playerWeldMag._magData.charge * weldedSurface.gameObject.GetComponent<MagnetComponentBase>()._magData.charge == -1 ? weldAimDir : -weldAimDir);
                    Quaternion targetWeldAimQuat = Quaternion.LookRotation(Vector3.forward, weldAimDir);
                    //rb.AddForceAtPosition(weldAimDir * 1000f, playerWeldMag.transform.position);

                    // Raycast to the point, get normal back.
                    RaycastHit2D[] hits = Physics2D.RaycastAll(playerWeldMag.transform.position + (-playerWeldMag.transform.up * .5f), weldAimDir, 1.5f, weldLayerMask);
                    foreach (RaycastHit2D hit in hits)
                    {
                        if (hit.collider == weldedSurface)
                        {
                            weldSurfaceNormal = hit.normal;
                        }
                    }

                    if (weldState != WeldState.Welded && weldState != WeldState.LaunchAim)
                    {
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
                // If on a neutral surface, use rigidbody for foddian movement or walk around.
                if (neutralDetector.neutralDetected)//positiveMag.affectFields.Count + negativeMag.affectFields.Count <= 0 && hits.Length > 0)
                {
                    intermediateRot = Quaternion.Slerp(intermediateRot, targetAimQuat, Time.deltaTime * rotationFactor);
                    rb.MoveRotation(intermediateRot);
                    if (velocity > 10f)
                    {
                        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity.normalized, 10f);
                    }
                }
                else
                {
                    // Manually Update Transforms to face software cursor.
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetAimQuat, Time.deltaTime * rotationFactor);
                    intermediateRot = transform.rotation;
                }

            }
            else if (weldState == WeldState.Welded)
            {
                Vector3 weldAimDir = (playerWeldMag.transform.position - cursorObj.transform.position).normalized;
                //adjustedWeldAimDir = (playerWeldMag == positiveMag ? -1f : 1f) * ((playerWeldMag.transform.position - cursorObj.transform.position).normalized);
                Quaternion weldTargetAimQuat = Quaternion.LookRotation(Vector3.forward, weldAimDir);

                //float angleFromSurfNormal = (float)System.Math.Round(Mathf.Atan2(weldSurfaceNormal.y, weldSurfaceNormal.x) - Mathf.Atan2(adjustedWeldAimDir.y, adjustedWeldAimDir.x), 2);
                intermediateRot = Quaternion.Slerp(intermediateRot, weldTargetAimQuat, Time.deltaTime * rotationFactor);
                rb.MoveRotation(intermediateRot);

                weldBlob.transform.position = playerWeldMag.transform.position + ((Vector3)weldSurfaceNormal * -.1f);
                weldBlob.transform.rotation = Quaternion.LookRotation(weldBlob.transform.forward, weldSurfaceNormal);
            }
            else if (weldState == WeldState.LaunchAim)
            {
                scalePivot.transform.localScale = new Vector3(FunctionLibraryF.MapRangeClamped(0.4f, 1f, 1f, 1.25f, softwareCursor.GetLaunchAlpha()), Mathf.Lerp(1f, .5f, softwareCursor.GetLaunchAlpha()), 1f);
                
                weldBlob.transform.position = playerWeldMag.transform.position + ((Vector3)weldSurfaceNormal * -.1f);
                weldBlob.transform.rotation = Quaternion.LookRotation(weldBlob.transform.forward, weldSurfaceNormal);
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
                    Vector2 curForce = positiveFields[i].GetAppliedForce(positiveMag._magData, positiveMag.transform.position, positiveMag._fieldAttractDistance, velocity);

                    // Prevent NaN
                    if (float.IsNaN(curForce.x))
                    {
                        curForce = Vector2.zero;
                    }

                    Vector2 adjustedAimDir = (positiveFields[i]._magData.charge * positiveMag._magData.charge == -1 ? positiveMagDir : -positiveMagDir);
                    // Will not be accounted for if pole is facing opposite direction (for game feel.)
                    if (Vector2.Dot(adjustedAimDir, curForce.normalized) >= 0f)
                    {
                        float aimDirInfluence = FunctionLibraryF.MapRangeClamped(0f, 10f, .6f, .3f, velocity);
                        combinedPositiveForces += (Vector2.Dot(adjustedAimDir, curForce.normalized) * Vector2.ClampMagnitude((curForce.magnitude * Vector2.Lerp(curForce, (adjustedAimDir * curForce.magnitude), aimDirInfluence).normalized), 100f));
                    }
                }
                #endregion

                #region Combine Negative Magnet Forces
                Vector2 combinedNegativeForces = Vector2.zero;
                Vector2 negativeMagDir = (negativeMag.transform.up).normalized;

                List<MagnetComponentBase> negativeFields = negativeMag.affectFields.ToList();
                for (int i = 0; i < negativeFields.Count; i++)
                {
                    Vector2 curForce = negativeFields[i].GetAppliedForce(negativeMag._magData, negativeMag.transform.position, negativeMag._fieldAttractDistance, velocity);

                    // Prevent NaN
                    if (float.IsNaN(curForce.x))
                    {
                        curForce = Vector2.zero;
                    }

                    Vector2 adjustedAimDir = (negativeFields[i]._magData.charge * negativeMag._magData.charge == -1 ? negativeMagDir : -negativeMagDir);
                    // Will not be accounted for if pole is facing opposite direction (for game feel.)
                    if (Vector2.Dot(adjustedAimDir, curForce.normalized) >= 0f)
                    {
                        float aimDirInfluence = FunctionLibraryF.MapRangeClamped(0f, 10f, .6f, .3f, velocity);
                        combinedNegativeForces += (Vector2.Dot(adjustedAimDir, curForce.normalized) * Vector2.ClampMagnitude((curForce.magnitude * Vector2.Lerp(curForce, (adjustedAimDir * curForce.magnitude), aimDirInfluence).normalized), 100f));
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
        else
        {
            // Manually Update Transforms to face software cursor.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetAimQuat, Time.deltaTime * rotationFactor);
            intermediateRot = transform.rotation;
        }
    }

    #region Input Actions
    public void DebugSkipToNextRoom()   // Remove this in builds.
    {
        if (Application.isEditor)
        {
            AreaManager.instance.ReachedObjective(GameObject.FindFirstObjectByType<roomObjective>().gameObject);
        }
    }

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
        // Break out of spawn cage.
        if (AreaManager.instance.IsTransitionState(AreaManager.AreaTransitionState.Spawn))
        {
            spawnCage.SetActive(false);
            AreaManager.instance.SetTransitionState(AreaManager.AreaTransitionState.None);
        }

        else
        {
            if (playerInput.currentControlScheme.Equals("Gamepad"))
            {

            }
            else
            {
                weldInput = (context.ReadValue<float>() == 1 ? true : false);
            }
        } 
    }
    
    #endregion
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
                weldedSurface = null;
                cursorObj.GetComponent<SoftwareCursor>().SetNewPosParent(gameObject);
                rotationFactor = 30f;
                positiveMag.GetComponent<HingeJoint2D>().enabled = false;
                negativeMag.GetComponent<HingeJoint2D>().enabled = false;
                weldBlob.SetActive(false);
                break;
            case WeldState.Welded:
                cursorObj.GetComponent<SoftwareCursor>().SetNewPosParent(playerWeldMag.gameObject);
                Vector2 adjustedWeldAimDir = (playerWeldMag == positiveMag ? -1f : 1f) * ((playerWeldMag.transform.position - cursorObj.transform.position).normalized);
                //float angleFromSurfNormal = (float)System.Math.Round(Mathf.Atan2(weldSurfaceNormal.y, weldSurfaceNormal.x) - Mathf.Atan2(adjustedWeldAimDir.y, adjustedWeldAimDir.x), 2);
                float angleFromSurfNormal = Vector3.SignedAngle(weldSurfaceNormal, -playerWeldMag.transform.up, Vector3.forward);

                // If weld is outside of constraint range, start coroutine to correct it.
                if (Mathf.Abs(angleFromSurfNormal) > weldAngleClamp || Mathf.Abs(angleFromSurfNormal) < -weldAngleClamp)
                {
                    StartCoroutine(softwareCursor.WeldJustStarted(.1f));
                }
                    
                rotationFactor = 60f;
                playerWeldMag.GetComponent<HingeJoint2D>().enabled = true;
                weldBlob.transform.position = playerWeldMag.transform.position + ((Vector3)weldSurfaceNormal * -.1f);
                weldBlob.transform.rotation = Quaternion.LookRotation(weldBlob.transform.forward, weldSurfaceNormal);
                weldBlob.SetActive(true);
                break;
            case WeldState.LaunchAim:
                rotationFactor = 0f;

                // Parent player to squash stretch pivot.
                UpdateScalePivotTransforms();

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

    public void UpdateScalePivotTransforms()
    {
        if (playerWeldMag != null)
        {
            scalePivot.transform.position = playerWeldMag.transform.position;
            scalePivot.transform.rotation = playerWeldMag.transform.rotation * Quaternion.Euler(0f, 0f, 180f);
        }
    }
    public void CancelLaunch(InputAction.CallbackContext context)
    {
        Debug.Log("Launch is cancelled.");
        ChangeWeldState(WeldState.Welded);
    }

    public void ResetUponNewRoom(Vector3 startPos)
    {
        gameObject.transform.position = startPos;
        spawnCage.SetActive(true);
        spawnCage.transform.position = gameObject.transform.position;
        rb.linearVelocity = new Vector2(0f, 0f);
        rb.gravityScale = 0f;
        rb.WakeUp();

        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>().SnapToTarget();
    }

    public void Restart()
    {
        transform.position = startPos;
        rb.linearVelocity = Vector3.zero;
    }

    public void AddParentSource(GameObject parentObj)
    {
        // For now, just supports one parent at a time. Unsure if will ever need multiple but that will be a challenge for a different time.
        surfaceParent = parentObj;
        parentLastPos = surfaceParent.transform.position;
        parentLastRot = surfaceParent.transform.rotation;
        softwareCursor.SetParentLastTransforms(parentLastPos, parentLastRot);
    }

    public void ClearParentSource()
    {
        // Player maintains parent velocity when leaving.
        if (surfaceParent != null)
        {
            rb.linearVelocity += (Vector2)surfaceParentVelocity;
        }

        surfaceParent = null;
    }

    #region Getters / Setters
    public void SetIntermediateRot(Quaternion inRot)
    {
        intermediateRot = inRot;
    }

    public Rigidbody2D GetRigidBody()
    {
        return rb;
    }

    public GameObject GetParentSource()
    {
        return surfaceParent;
    }

    #endregion
}
