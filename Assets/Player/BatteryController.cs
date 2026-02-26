using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using System.Linq;
using Magnet;
using FunctionLibrary;
using UnityEditor.Experimental.GraphView;
using Unity.VisualScripting;

public class BatteryController : MonoBehaviour
{
    #region Properties
    [Header("References")]
    [SerializeField] private GameObject scalePivot;
    [SerializeField] private GameObject weldBlob;
    [SerializeField] private GameObject spawnCage;
    [SerializeField] private GameObject scoutTarget;
    private Camera mainCam;
    private Rigidbody2D rb;
    private Collider2D surfaceCol;
    private PlayerInput playerInput;
    private PlayerNeutralDetector neutralDetector;  
    private GameObject cursorObj;
    private SoftwareCursor softwareCursor;
    public MagneticSurface positiveMag;
    public TractorBeamVFX posTractorBeamVFX;
    public MagneticSurface negativeMag;
    public TractorBeamVFX negTractorBeamVFX;

    [Header("Control Settings")]
    [SerializeField] private float rotationFactor;

    // Weld
    public float weldAngleClamp = 75f;
    [SerializeField] private LayerMask weldLayerMask;

    // Launch
    [SerializeField] private float launchMaxExponent = 1.13f;
    [SerializeField] private float launchBaseConstant = 500f;

    // Battery
    [HideInInspector] public Battery battery;

    [HideInInspector] public float playerGravity;
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
    private Vector2 playerColOffset;
    private Vector2 playerColSize;

    // Intermediary
    [HideInInspector] public float velocity;
    [HideInInspector] public float angularVelocity;
    private Quaternion previousRotation; /// Used for calculating angular velocity.
    private Vector3 previousWeldUp;    /// Used for determining input rotation direction.
    private Quaternion intermediateRot; /// Used for interpolating the rigidbody rotation of the player.

    // Scouting
    private Vector2 scoutInput;
    private Vector2 scoutPosOffset = Vector2.zero;
    Vector2 scoutVelocity = Vector2.zero;
    float scoutDistance = 10f;
    private enum ScoutState
    {
        None,
        Scouting,
        Returning
    }
    private ScoutState scoutState;

    // Input System
    private bool weldInput;
    private bool launchInput;

    private Vector3 startPos; /// Used for current restart. REMOVE THIS LATER when restart is tied into AreaManager/PlayerStart stuff.
    #endregion
    // =============================================================================================================================
    #endregion

    private void Awake()
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
        playerGravity = rb.gravityScale;
        battery = GetComponent<Battery>();
        neutralDetector = GetComponentInChildren<PlayerNeutralDetector>();

        playerColOffset = gameObject.GetComponent<CapsuleCollider2D>().offset;
        playerColSize = gameObject.GetComponent<CapsuleCollider2D>().size;
        #endregion
    }
    void Start()
    {
        #region Initialize Variables
        previousRotation = transform.rotation;
        previousWeldUp = -negativeMag.transform.up;
        intermediateRot = transform.rotation;
        #endregion

        #region Bind Delegates
        battery.onCorrode += Death;
        #endregion
    }

    private void FixedUpdate()
    {
        var dt = Time.fixedDeltaTime;

        #region Inherit Surface Parent Delta
        //Debug.Log(gameObject.transform.parent + " is parent. " + surfaceParent + " is surfaceParent.");
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
                //Debug.Log("ScalePivot is parent.");
                scalePivot.transform.position += surfaceParentPosDelta;
                scalePivot.transform.RotateAround(parentPivot, Vector3.forward, rotAngle);
                //scalePivot.transform.position += surfaceParentPosDelta;
                //scalePivot.transform.RotateAround(parentPivot, Vector3.forward, rotAngle);
                //Physics2D.SyncTransforms();
            }
            else
            {
                // This works, but modifies transforms directly which causes rigidbody operations to fail.
                transform.position += surfaceParentPosDelta;
                transform.RotateAround(parentPivot, Vector3.forward, rotAngle);
                Physics2D.SyncTransforms(); /// Necessary.
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
            #region Weld To Surface / Update Weld Surface Data
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
                    /// (Somewhat expensive, but can't be avoided as this needs done every frame to account for rotating/translating parent surfaces as well as sudden changes to level state.)
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
            else if (!weldInput && weldState == WeldState.None && !lockWeldState)
            {
                #region Reduce Gravity When Touching Surface (Game Feel for Climbing / Sticking)
                Collider2D evalAttractSurface = null;

                Collider2D[] positiveOverlap = Physics2D.OverlapCircleAll((Vector2)positiveMag.transform.position, .3f, weldLayerMask);
                Collider2D[] negativeOverlap = Physics2D.OverlapCircleAll((Vector2)negativeMag.transform.position, .3f, weldLayerMask);

                // Positive
                if (evalAttractSurface == null)
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
                                evalAttractSurface = curMag.gameObject.GetComponent<Collider2D>();
                                break;
                            }
                        }
                    }
                }

                // Negative
                if (evalAttractSurface == null)
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
                                evalAttractSurface = curMag.gameObject.GetComponent<Collider2D>();
                                break;
                            }
                        }
                    }
                }

                // Reduce gravity
                if (evalAttractSurface != null)
                {
                    rb.gravityScale = playerGravity * 0f;
                    rb.linearDamping = 20f;
                }
                else
                {
                    rb.gravityScale = playerGravity;
                    rb.linearDamping = .01f;
                }
                #endregion
                //Debug.Log(rb.gravityScale);
            }
            else
            {
                // This is so can still launch off without damping and gravity being different.
                rb.gravityScale = playerGravity;
                rb.linearDamping = .01f;
            }
            #endregion

            #region Weld State Update Functionality
            if (weldState == WeldState.None)
            {
                // If on a neutral surface, use rigidbody for foddian movement or walk around.
                if (neutralDetector.neutralDetected)
                {
                    //Debug.Log("Neutral is detected!");
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
                Quaternion weldTargetAimQuat = Quaternion.LookRotation(Vector3.forward, weldAimDir);

                intermediateRot = Quaternion.Slerp(intermediateRot, weldTargetAimQuat, Time.deltaTime * rotationFactor);
                rb.MoveRotation(intermediateRot);

                scalePivot.transform.localScale = new Vector3(FunctionLibraryF.MapRangeClamped(0.4f, 1f, 1f, 1.25f, softwareCursor.GetLaunchAlpha()), Mathf.Lerp(1f, .5f, softwareCursor.GetLaunchAlpha()), 1f);
                scalePivot.transform.rotation = intermediateRot;

                weldBlob.transform.position = playerWeldMag.transform.position + ((Vector3)weldSurfaceNormal * -.1f);
                weldBlob.transform.rotation = Quaternion.LookRotation(weldBlob.transform.forward, weldSurfaceNormal);

                // Tractor Beam VFX
                posTractorBeamVFX.SetEndPos(positiveMag.transform.position);
                negTractorBeamVFX.SetEndPos(negativeMag.transform.position);
                posTractorBeamVFX.SetForceMagnitude(0f);
                negTractorBeamVFX.SetForceMagnitude(0f);
            }
            else if (weldState == WeldState.LaunchAim)
            {
                Vector3 weldAimDir = (playerWeldMag.transform.position - cursorObj.transform.position).normalized;
                Quaternion weldTargetAimQuat = Quaternion.LookRotation(Vector3.forward, (playerWeldMag == positiveMag ? -1f * weldAimDir : weldAimDir));

                ////Physics2D.SyncTransforms();
                intermediateRot = Quaternion.Slerp(intermediateRot, weldTargetAimQuat, Time.deltaTime * rotationFactor);
                rb.MoveRotation(intermediateRot);

                // Adjust scale pivot rotation and scale.
                scalePivot.transform.localScale = new Vector3(FunctionLibraryF.MapRangeClamped(0.4f, 1f, 1f, 1.25f, softwareCursor.GetLaunchAlpha()), Mathf.Lerp(1f, .5f, softwareCursor.GetLaunchAlpha()), 1f);
                scalePivot.transform.rotation = intermediateRot;

                // Update player collider size and offset to be even smaller as they squish smaller. This prevents bugs with the rotation causing welding to force exit.
                gameObject.GetComponent<CapsuleCollider2D>().size = new Vector2(FunctionLibraryF.MapRangeClamped(0.2f, 1f, playerColSize.x * .8f, playerColSize.x * .25f, softwareCursor.GetLaunchAlpha()), playerColSize.y * .8f);
                gameObject.GetComponent<CapsuleCollider2D>().offset = new Vector2(playerColOffset.x, ((playerWeldMag == positiveMag) ? -1f : 1f) * (playerColOffset.y + FunctionLibraryF.MapRangeClamped(0.2f, 1f, 0.2f, .22f, softwareCursor.GetLaunchAlpha())));

                weldBlob.transform.position = playerWeldMag.transform.position + ((Vector3)weldSurfaceNormal * -.1f);
                weldBlob.transform.rotation = Quaternion.LookRotation(weldBlob.transform.forward, weldSurfaceNormal);

                // Tractor Beam VFX
                posTractorBeamVFX.SetEndPos(positiveMag.transform.position);
                negTractorBeamVFX.SetEndPos(negativeMag.transform.position);
                posTractorBeamVFX.SetForceMagnitude(0f);
                negTractorBeamVFX.SetForceMagnitude(0f);
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
                    Vector2 curForce = positiveFields[i].GetAppliedForce(positiveMag._magData, positiveMag.transform.position, positiveMag._fieldAttractDistance, rb.linearVelocity);

                    // Prevent NaN
                    if (float.IsNaN(curForce.x))
                    {
                        curForce = Vector2.zero;
                    }

                    Vector2 adjustedAimDir = (positiveFields[i]._magData.charge * positiveMag._magData.charge == -1 ? positiveMagDir : -positiveMagDir);
                    // Will not be accounted for if pole is facing opposite direction (for game feel.)
                    if (Vector2.Dot(adjustedAimDir, curForce.normalized) >= 0f)
                    {
                        //float aimDirInfluenceAlpha = FunctionLibraryF.MapRangeClamped(0f, 10f, .75f, .3f, velocity);
                        float aimDirInfluenceAlpha = FunctionLibraryF.MapRangeClamped(0f, 1f, 0f, 1f, Vector2.Dot(adjustedAimDir, curForce.normalized));
                        combinedPositiveForces += (Vector2.Dot(adjustedAimDir, curForce.normalized) * Vector2.ClampMagnitude((curForce.magnitude * Vector2.Lerp(curForce, (adjustedAimDir * curForce.magnitude), aimDirInfluenceAlpha).normalized), 80f));
                    }
                }
                #endregion

                #region Combine Negative Magnet Forces
                Vector2 combinedNegativeForces = Vector2.zero;
                Vector2 negativeMagDir = (negativeMag.transform.up).normalized;

                List<MagnetComponentBase> negativeFields = negativeMag.affectFields.ToList();
                for (int i = 0; i < negativeFields.Count; i++)
                {
                    Vector2 curForce = negativeFields[i].GetAppliedForce(negativeMag._magData, negativeMag.transform.position, negativeMag._fieldAttractDistance, rb.linearVelocity);
                    // Prevent NaN
                    if (float.IsNaN(curForce.x))
                    {
                        curForce = Vector2.zero; 
                    }

                    Vector2 adjustedAimDir = (negativeFields[i]._magData.charge * negativeMag._magData.charge == -1 ? negativeMagDir : -negativeMagDir);
                    // Will not be accounted for if pole is facing opposite direction (for game feel.)
                    if (Vector2.Dot(adjustedAimDir, curForce.normalized) >= 0f)
                    {
                        //float aimDirInfluenceAlpha = FunctionLibraryF.MapRangeClamped(0f, 10f, .75f, .3f, velocity);
                        float aimDirInfluenceAlpha = FunctionLibraryF.MapRangeClamped(0f, 1f, 0f, 1f, Vector2.Dot(adjustedAimDir, curForce.normalized));
                        combinedNegativeForces += (Vector2.Dot(adjustedAimDir, curForce.normalized) * Vector2.ClampMagnitude((curForce.magnitude * Vector2.Lerp(curForce, (adjustedAimDir * curForce.magnitude), aimDirInfluenceAlpha).normalized), 80f));
                    }
                }
                #endregion

                // Apply Forces

                float velocityMultiplier = 1f;// FunctionLibraryF.MapRangeClamped(0f, 10f, .8f, 1.15f, velocity);
                float angularMultiplier = 1f;//FunctionLibraryF.MapRangeClamped(0f, 25f, 1f, 1.25f, Mathf.Abs(angularVelocity));

                // Positive
                rb.AddForceAtPosition(combinedPositiveForces * velocityMultiplier * angularMultiplier, positiveMag.transform.position);
                //Debug.DrawLine(positiveMag.transform.position, (Vector2)positiveMag.transform.position + (combinedPositiveForces * .25f));

                if (positiveFields.Count > 0)
                {
                    Vector2 posEndPos;
                    RaycastHit2D posHit = Physics2D.Raycast(positiveMag.transform.position, (combinedPositiveForces.normalized * ((Vector2.Dot(positiveMag.transform.up, combinedPositiveForces.normalized) >= 0) ? 1 : -1)), 100f, 1 << 8);
                    if (posHit)
                    {
                        posEndPos = posHit.point;
                    }
                    else
                    {
                        posEndPos = (Vector2)positiveMag.transform.position + ((combinedPositiveForces.normalized * -5f));
                    }
                    posTractorBeamVFX.SetEndPos(posEndPos);
                    posTractorBeamVFX.SetForceMagnitude(combinedPositiveForces.magnitude);
                }
                else
                {
                    posTractorBeamVFX.SetEndPos(positiveMag.transform.position);
                    posTractorBeamVFX.SetForceMagnitude(0f);
                }

                // Negative
                rb.AddForceAtPosition(combinedNegativeForces * velocityMultiplier * angularMultiplier, negativeMag.transform.position);
                //Debug.DrawLine(negativeMag.transform.position, (Vector2)negativeMag.transform.position + (combinedNegativeForces * .25f));

                if (negativeFields.Count > 0)
                {
                    Vector2 negEndPos;
                    RaycastHit2D negHit = Physics2D.Raycast(negativeMag.transform.position, (combinedNegativeForces.normalized * ((Vector2.Dot(negativeMag.transform.up, combinedNegativeForces.normalized) >= 0) ? 1 : -1)), 100f, 1 << 8);
                    if (negHit)
                    {
                        negEndPos = negHit.point;
                    }
                    else
                    {
                        negEndPos = (Vector2)negativeMag.transform.position + ((combinedNegativeForces.normalized * -5f));
                    }
                    //Debug.DrawLine(negEndPos, negEndPos + new Vector2(0f, .5f), Color.darkSalmon, .1f);
                    negTractorBeamVFX.SetEndPos(negEndPos);
                    negTractorBeamVFX.SetForceMagnitude(combinedNegativeForces.magnitude);
                }
                else
                {
                    negTractorBeamVFX.SetEndPos(negativeMag.transform.position);
                    negTractorBeamVFX.SetForceMagnitude(0f);
                }
                

            }
            #endregion

            #region Scouting
            if (scoutState == ScoutState.Scouting)
            {
                scoutPosOffset = Vector2.SmoothDamp(scoutPosOffset, Vector2.ClampMagnitude(scoutPosOffset + (scoutInput * 9999999f * Time.fixedDeltaTime), scoutDistance), ref scoutVelocity, .3f);
            }
            else if (scoutState == ScoutState.Returning)
            {
                scoutPosOffset = Vector2.SmoothDamp(scoutPosOffset, Vector2.zero, ref scoutVelocity, .2f);
                Debug.Log(scoutPosOffset);
                if (FunctionLibraryF.VectorsApproximatelyEqual(scoutPosOffset, Vector2.zero))
                {
                    scoutState = ScoutState.None;
                    scoutTarget.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    CameraManager.instance.RemoveFollowTarget(scoutTarget.transform);
                    scoutPosOffset = Vector2.zero;
                }
            }
            
            scoutTarget.transform.position = gameObject.transform.position + (Vector3)scoutPosOffset;
            #endregion
        }
        else
        {
            // Manually Update Transforms to face software cursor.
            //transform.rotation = Quaternion.Slerp(transform.rotation, targetAimQuat, Time.deltaTime * rotationFactor);
            //intermediateRot = transform.rotation;
        }

        #region Update Global Shader Values
        // TO DO: Look into using VectorArrays so that other objects can also affect the magnetic field (not just the two player magnets)
        Shader.SetGlobalVector("_PlayerPosMag", positiveMag.transform.position);
        Shader.SetGlobalVector("_PlayerNegMag", negativeMag.transform.position);
        Shader.SetGlobalVector("_PlayerNegMagScreen", Camera.main.WorldToScreenPoint(negativeMag.transform.position));

        #endregion
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

    public void Scout(InputAction.CallbackContext context)
    {
        if (playerInput.currentControlScheme.Equals("Gamepad"))
        {
            scoutInput = context.ReadValue<Vector2>();
        }
        else
        {
            if (context.started)
            {
                scoutState = ScoutState.Scouting;
                CameraManager.instance.AddFollowTarget(scoutTarget.transform);
                scoutTarget.gameObject.GetComponent<SpriteRenderer>().enabled = true;
            }
            else if (context.canceled)
            {
                scoutState = ScoutState.Returning;
            }
            scoutInput = context.ReadValue<Vector2>();
        }
    }

    public void Weld(InputAction.CallbackContext context)
    {
        // Break out of spawn mechanism.
        if (AreaManager.instance.IsTransitionState(AreaManager.AreaTransitionState.Spawn))
        {
            //spawnCage.SetActive(false);
            AreaManager.instance.ReleasePlayer();
        }

        else
        {
            if (context.canceled)
            {
                if (weldState == WeldState.LaunchAim && playerWeldMag == positiveMag)
                {
                    softwareCursor.InvertLocalPos();
                }
                ChangeWeldState(WeldState.None);
            }

            if (playerInput.currentControlScheme.Equals("Gamepad"))
            {

            }
            else
            {
                weldInput = (context.ReadValue<float>() == 1 ? true : false);
            }
        }
    }

    public void InitiateLaunch(InputAction.CallbackContext context)
    {
        launchInput = (context.ReadValue<float>() == 1 ? true : false);

        if (context.canceled && weldState == WeldState.LaunchAim)
        {
            if (softwareCursor.GetLaunchAlpha() <= .1f)
            {
                CancelLaunch();
            }
            else
            {
                if (battery.GetPercent() >= 25)
                {
                    LaunchFromWeld();
                }
                else
                {
                    Debug.Log("Not enough battery!");
                    CancelLaunch();
                }
            }
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
                    if (weldState == WeldState.Welded)
                    {
                        ChangeWeldState(WeldState.LaunchAim);
                    }
                }
            }
        }
    }
    public void CancelLaunch(InputAction.CallbackContext context)
    {
        CancelLaunch();  
    }

    private void CancelLaunch()
    {
        if (weldState == WeldState.LaunchAim)
        {
            //Debug.Log("Launch is cancelled.");
            if (weldState == WeldState.LaunchAim && playerWeldMag == positiveMag)
            {
                softwareCursor.InvertLocalPos();
            }

            if (weldInput)
            {
                ChangeWeldState(WeldState.Welded);
            }
            else
            {
                ChangeWeldState(WeldState.None);
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

        #region Exec Based on Exitting Specific State
        // When leaving weld
        if (weldState == WeldState.Welded)
        {
            #region Handle Player Data
            // Reset Capsule Collider
            gameObject.GetComponent<CapsuleCollider2D>().offset = playerColOffset;
            gameObject.GetComponent<CapsuleCollider2D>().size = playerColSize;

            // Unparent from scale pivot.
            scalePivot.transform.localScale = Vector3.one;
            gameObject.transform.parent = scalePivot.transform.parent;

            // Clamp rigidbody velocity
            if (surfaceParent != null)
            {
                rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, 5f + surfaceParentVelocity.magnitude);
            }
            else
            {
                rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, 5f);
            }
            #endregion

            // Update camera follow targets.
            CameraManager.instance.AddFollowTarget(gameObject.transform, 0.5f);
            CameraManager.instance.RemoveFollowTarget(playerWeldMag.transform, 0.5f);
        }

        // When leaving Launch Aim, unparent player from the squash/stretch pivot.
        if (weldState == WeldState.LaunchAim)
        {
            #region Handle Player Data
            // Reset Capsule Collider Offset
            gameObject.GetComponent<CapsuleCollider2D>().offset = playerColOffset;
            gameObject.GetComponent<CapsuleCollider2D>().size = playerColSize;

            scalePivot.transform.localScale = Vector3.one;
            gameObject.transform.parent = scalePivot.transform.parent;
            #endregion
        }
        #endregion

        weldState = newState;

        #region Exec Based on Entering Specific State
        switch (weldState)
        {
            case WeldState.None:
                weldedSurface = null;
                cursorObj.GetComponent<SoftwareCursor>().SetNewPosParent(gameObject);
                rotationFactor = 30f;
                gameObject.GetComponent<HingeJoint2D>().enabled = false;
                gameObject.GetComponent<HingeJoint2D>().enabled = false;
                weldBlob.SetActive(false);
                break;
            case WeldState.Welded:
                cursorObj.GetComponent<SoftwareCursor>().SetNewPosParent(playerWeldMag.gameObject);
                Vector2 adjustedWeldAimDir = (playerWeldMag == positiveMag ? -1f : 1f) * ((playerWeldMag.transform.position - cursorObj.transform.position).normalized);
                float angleFromSurfNormal = Vector3.SignedAngle(weldSurfaceNormal, -playerWeldMag.transform.up, Vector3.forward);

                // If weld is outside of constraint range, start coroutine to correct it.
                if (Mathf.Abs(angleFromSurfNormal) > weldAngleClamp || Mathf.Abs(angleFromSurfNormal) < -weldAngleClamp)
                {
                    StartCoroutine(softwareCursor.WeldJustStarted(.1f));
                }
                    
                rotationFactor = 15f;
                UpdateScalePivotTransforms();
                gameObject.transform.parent = scalePivot.transform;
                // Set anchor data for the hingejoint
                gameObject.GetComponent<HingeJoint2D>().enabled = true;

                // Adjust capsule collider.
                gameObject.GetComponent<CapsuleCollider2D>().offset = playerColOffset + new Vector2(0f, ((playerWeldMag == positiveMag) ? -.15f : .15f));
                gameObject.GetComponent<CapsuleCollider2D>().size = playerColOffset + new Vector2(playerColSize.x * .8f, playerColSize.y * .8f);

                weldBlob.transform.position = playerWeldMag.transform.position + ((Vector3)weldSurfaceNormal * -.1f);
                weldBlob.transform.rotation = Quaternion.LookRotation(weldBlob.transform.forward, weldSurfaceNormal);
                weldBlob.SetActive(true);

                // Update camera follow target.
                CameraManager.instance.AddFollowTarget(playerWeldMag.transform, 0.5f);
                CameraManager.instance.RemoveFollowTarget(gameObject.transform, 0.5f);

                break;
            case WeldState.LaunchAim:

                // Adjust capsule collider.
                gameObject.GetComponent<CapsuleCollider2D>().offset = new Vector2(playerColOffset.x, ((playerWeldMag == positiveMag) ? -1f : 1f) * (playerColOffset.y + FunctionLibraryF.MapRangeClamped(0.2f, 1f, 0.2f, .22f, softwareCursor.GetLaunchAlpha())));
                gameObject.GetComponent<CapsuleCollider2D>().size = new Vector2(FunctionLibraryF.MapRangeClamped(0.2f, 1f, playerColSize.x * .8f, playerColSize.x * .25f, softwareCursor.GetLaunchAlpha()), playerColSize.y * .8f);

                softwareCursor.LaunchAimStarted();

                // Parent player to squash stretch pivot.
                UpdateScalePivotTransforms();
                gameObject.transform.parent = scalePivot.transform;

                rb.gravityScale = playerGravity;
                rb.linearDamping = .01f;

                break;
            default:
                rotationFactor = 30f;
                break;
        }
        #endregion
    }

    public IEnumerator LockWeldState(float duration)
    {
        lockWeldState = true;
        yield return new WaitForSeconds(duration);
        lockWeldState = false;
        yield break;
    }

    public void LaunchFromWeld()
    {
        if (softwareCursor.launchAimControlMethod == SoftwareCursor.LaunchAimControlMethods.FreeRange && playerWeldMag == positiveMag)
        {
            softwareCursor.InvertLocalPos();
        }
            
        ChangeWeldState(WeldState.None);
        //weldInput = false; /// Flush input (uncommenting means players will need to repress space to weld to another surface after launch.)
        StartCoroutine(LockWeldState(.2f));

        battery.SubtractPercent(FunctionLibraryF.MapRangeClamped(.25f, 1f, 5f, 25f, softwareCursor.GetLaunchAlpha()));

        float launchForce = Mathf.Pow(launchBaseConstant, FunctionLibraryF.MapRangeClamped(0f, 1f, 1f, launchMaxExponent, softwareCursor.GetLaunchAlpha()));
        rb.AddForceAtPosition(-playerWeldMag.transform.up * launchForce, playerWeldMag.transform.position);

        
    }

    public void UpdateScalePivotTransforms()
    {
        if (playerWeldMag != null)
        {
            scalePivot.transform.position = playerWeldMag.transform.position;

            /// Leaving this here as a warning NOT to use the same code that is in FixedUpdate to update orientation here. The manual rotation set below works way better for here, the other breaks stuff.
            #region Do Not Use!
            //Vector3 weldAimDir = (playerWeldMag.transform.position - cursorObj.transform.position).normalized;
            //Quaternion weldTargetAimQuat = Quaternion.LookRotation(Vector3.forward, weldAimDir);

            //intermediateRot = Quaternion.Slerp(intermediateRot, weldTargetAimQuat, Time.deltaTime * rotationFactor);
            //scalePivot.transform.rotation = intermediateRot;
            #endregion

            scalePivot.transform.rotation = (playerWeldMag == negativeMag ? playerWeldMag.transform.rotation * Quaternion.Euler(0f, 0f, 180f) : playerWeldMag.transform.rotation);
            Physics2D.SyncTransforms();
        }
    }

    public void ResetUponNewRoom(Vector3 newStartPos)
    {
        startPos = newStartPos;
        gameObject.transform.position = startPos;
        //spawnCage.SetActive(true);
        //spawnCage.transform.position = gameObject.transform.position;
        rb.linearVelocity = new Vector2(0f, 0f);
        rb.gravityScale = 0f;
        rb.WakeUp();
    }

    public void RestartInput(InputAction.CallbackContext context)
    {
        if (context.performed) /// If held for duration specified in the Input Settings.
        {
            Restart();
        }       
    }
    public void Restart()
    {
        // TO DO: Should this reload room completely? Need to go about resetting the state of the room.
        // NOTE: Make sure restarting does NOT use up a player life.

        transform.position = startPos;
        rb.linearVelocity = Vector3.zero;
    }

    public void Death()
    {
        // TO DO: Ensure reset room state when respawning in the same room.
        // TO DO: Reset battery percentage to be what is was upon entering the room.
        // TO DO: Checkpoint logic
        if (GameInstance.instance.playerLives <= 0)
        {
            AreaManager.instance.Respawn();
        }
        else
        {
            GameInstance.instance.SetPlayerLives((byte)(GameInstance.instance.playerLives - 1));
            Restart();
        }    
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        ICollect collectScript = collision.gameObject.GetComponent<ICollect>();
        if (collectScript != null)
        {
            collectScript.Collect(surfaceCol);
        }
    }
    #region Parent Source
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
    #endregion

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
