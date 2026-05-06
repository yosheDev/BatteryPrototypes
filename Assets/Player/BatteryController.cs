using FunctionLibrary;
using Magnet;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.Image;

public class BatteryController : MonoBehaviour, IDamageable
{
    #region Properties
    #region References
    [Header("References")]
    [SerializeField] private GameObject scalePivot;
    [SerializeField] private GameObject weldBlob;
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
    private FerroProjectilePool projectilePool;
    [SerializeField] private SpringJoint2D grappleJoint;
    [SerializeField] private GrappleRendererVFX grappleRenderer;
    [SerializeField] private PlayerSpriteManager spriteManager;
    [SerializeField] private PlayerFastRotationPreventer posPreventer;
    [SerializeField] private PlayerFastRotationPreventer negPreventer;
    #endregion

    [Header("Control Settings")]
    [SerializeField] private float rotationFactor;              /// Is this deprecated now? Or still used for magnetic surfaces movement.
    [SerializeField] private float rotationTorqueFactor = 8f;
    [SerializeField] private float rotationTorqueDrag = .2f;
    private float angularRotVelocity; // Used for smooth damp.
    [HideInInspector] private bool isGrounded = false;
    public enum PlayerInputMode
    {
        Disabled,
        UIOnly,
        Scene,
        Enabled
    }

    public PlayerInputMode inputMode = PlayerInputMode.Enabled;

    private bool isDead = false;

    #region Abilities

    private byte abilityProgression = 0;    /// Since ability progression is linear, using a byte to store what player has.


    #region Welding
    // TO DO: Put this into its own component or serializable class.
    public float weldAngleClamp = 75f;
    [SerializeField] private LayerMask weldLayerMask;
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

    #endregion

    #region Launching
    // TO DO: Put this into its own component or serializable class.
    [SerializeField] private float launchMaxExponent = 1.13f;
    [SerializeField] private float launchBaseConstant = 500f;
    private float launchBatteryDrain = 20;
    #endregion Launching

    #region Projectile
    // TO DO: Put this into its own component or serializable class.
    private bool isShootDisabled = false;
    private byte shotBatteryDrain = 5;
    #endregion

    #region Grappling
    [SerializeField] private float grappleRange = 8f;
    private bool isGrappling = false;
    private Transform grapplePoint;
    #endregion

    #endregion

    // Battery
    [HideInInspector] public Battery battery;

    [HideInInspector] public float playerGravity;
    
    #region Audio
    [Header("Audio")]
    // TO DO: Make a player audio component or script to handle all these references.
    [SerializeField] private AudioSource sfxSurfaceGeneric;
    [SerializeField] private AudioSource sfxSurfaceMagnet;
    [SerializeField] private AudioSource sfxMagneticBeam;
    [SerializeField] private AudioSource sfxFerroProjectile;
    #endregion
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

    // Interaction
    [HideInInspector] public IInteractable interactObj;

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
        surfaceCol = GetComponent<BoxCollider2D>();
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
        playerGravity = rb.gravityScale;
        battery = GetComponent<Battery>();
        neutralDetector = GetComponentInChildren<PlayerNeutralDetector>();
        projectilePool = GetComponent<FerroProjectilePool>();

        playerColOffset = gameObject.GetComponent<BoxCollider2D>().offset;
        playerColSize = gameObject.GetComponent<BoxCollider2D>().size;
        #endregion
    }
    void Start()
    {
        #region Initialize Variables
        previousRotation = transform.rotation;
        previousWeldUp = -negativeMag.transform.up;
        intermediateRot = transform.rotation;

        abilityProgression = GameInstance.instance.playerAbilityProgression;
        spriteManager.UpdateSpriteByProgression(abilityProgression);
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

        #region Passive Battery Regeneration
        if (velocity <= .1f)
        {
            battery.regenerationRate = .75f;
        }
        else
        {
            battery.regenerationRate = 1.5f;
        }
        #endregion

        // If Player should be able to move around.
        if (AreaManager.instance.IsTransitionState(AreaManager.AreaTransitionState.None) && inputMode == PlayerInputMode.Enabled)
        {
            #region Determine Grounded State
            if (weldState != WeldState.None)
            {
                isGrounded = true;
                isShootDisabled = false;
            }
            else
            {
                if (neutralDetector.neutralDetected)
                {
                    RaycastHit2D groundedHit = Physics2D.Raycast(rb.position, Vector2.down, 10f, (int)LayerMask.GetMask("Default", "PhysicsBone"));//(1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("PhysicsBone")));
                    if (groundedHit)
                    {
                        if (Vector2.Dot(groundedHit.normal, Vector2.up) >= 0.5f)
                        {
                            isGrounded = true;
                            isShootDisabled = false;
                        }
                    }
                    else
                    {
                        isGrounded = false;
                    }
                }
                else
                {
                    RaycastHit2D attachMagHit = Physics2D.Raycast(rb.position, Vector2.down, 1.35f, (int)LayerMask.GetMask("MagnetSurface", "PhysicsBone"));//1 << LayerMask.NameToLayer("MagnetSurface"));
                    if (attachMagHit)
                    {
                        isGrounded = true;
                        isShootDisabled = false;
                    }

                    isGrounded = false;
                    if (positiveMag.affectFields.Count + negativeMag.affectFields.Count > 0)
                    {
                        // Speed up shoot recovery rate.
                    }
                    else
                    {
                        // Return shoot recovery rate to normal.
                    }

                }
            }

            #endregion

            Collider2D evalAttractSurface = null; /// Used for non-welding states to know if they are pressed against a magnet surface or not.
            Collider2D magForwardSurface = null;

            #region Detect Magnets
            if (abilityProgression > 0) // Does player have magnetism unlocked?
            {
                #region Weld To Surface / Update Weld Surface Data / Reduce Gravity when Climbing
                // If Weld is inputted.
                if (weldInput)
                {
                    #region Weld
                    Collider2D evalWeldSurface = null; /// Surface of the magnet to cling to.
                    bool magCharge = false;            /// The charge of the players magnet that is clinging.

                    #region Get Cling Magnet Surface
                    Collider2D[] positiveOverlap = Physics2D.OverlapCircleAll((Vector2)positiveMag.transform.position, .3f, weldLayerMask);
                    Collider2D[] negativeOverlap = Physics2D.OverlapCircleAll((Vector2)negativeMag.transform.position, .3f, weldLayerMask);
                    //Debug.DrawLine(negativeMag.transform.position, negativeMag.transform.position + (negativeMag.transform.up * .3f), Color.yellow, .5f);
                    //Debug.DrawLine(negativeMag.transform.position, negativeMag.transform.position + (-negativeMag.transform.up * .3f), Color.yellow, .5f); Debug.DrawLine(negativeMag.transform.position, negativeMag.transform.position + (negativeMag.transform.up * .3f), Color.yellow, .5f);
                    //Debug.DrawLine(negativeMag.transform.position, negativeMag.transform.position + (negativeMag.transform.right * .3f), Color.yellow, .5f);
                    //Debug.DrawLine(negativeMag.transform.position, negativeMag.transform.position + (-negativeMag.transform.right * .3f), Color.yellow, .5f);

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
                    #region Reduce Gravity When Touching Mag Surface (Game Feel for Climbing / Sticking)


                    Collider2D[] positiveOverlap = Physics2D.OverlapCircleAll((Vector2)positiveMag.transform.position + ((Vector2)positiveMag.transform.up * .05f), .5f, weldLayerMask);
                    Collider2D[] negativeOverlap = Physics2D.OverlapCircleAll((Vector2)negativeMag.transform.position + ((Vector2)negativeMag.transform.up * .05f), .5f, weldLayerMask);
                    #region Debug Circle
                    //Debug.DrawLine((Vector2)negativeMag.transform.position + ((Vector2)negativeMag.transform.up * .05f), (Vector2)negativeMag.transform.position + ((Vector2)negativeMag.transform.up * .05f) + (-(Vector2)negativeMag.transform.up * .5f), Color.green, .5f);
                    //Debug.DrawLine((Vector2)negativeMag.transform.position + ((Vector2)negativeMag.transform.up * .05f), (Vector2)negativeMag.transform.position + ((Vector2)negativeMag.transform.up * .05f) + ((Vector2)negativeMag.transform.up * .5f), Color.green, .5f);
                    //Debug.DrawLine((Vector2)negativeMag.transform.position + ((Vector2)negativeMag.transform.up * .05f), (Vector2)negativeMag.transform.position + ((Vector2)negativeMag.transform.up * .05f) + (-(Vector2)negativeMag.transform.right * .5f), Color.green, .5f);
                    //Debug.DrawLine((Vector2)negativeMag.transform.position + ((Vector2)negativeMag.transform.up * .05f), (Vector2)negativeMag.transform.position + ((Vector2)negativeMag.transform.up * .05f) + ((Vector2)negativeMag.transform.right * .5f), Color.green, .5f);
                    #endregion

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
                }
                else
                {
                    // This is so can still launch off without damping and gravity being different.
                    rb.gravityScale = playerGravity;
                    rb.linearDamping = .01f;
                }
                #endregion

                #region Prevent Neutral Leap Combining With Magnet Forces
                // Debug Circle in the forward direction of the magnets. If mag surface detected, prevent neutral movement later on with a null check.
                Collider2D[] posForOverlap = Physics2D.OverlapCircleAll((Vector2)positiveMag.transform.position + ((Vector2)positiveMag.transform.up * .25f), .5f, weldLayerMask);
                Collider2D[] negForOverlap = Physics2D.OverlapCircleAll((Vector2)negativeMag.transform.position + ((Vector2)negativeMag.transform.up * .25f), .5f, weldLayerMask);
                #region Debug Circle
                //Debug.DrawLine((Vector2)negativeMag.transform.position + ((Vector2)negativeMag.transform.up * .25f) + ((Vector2)negativeMag.transform.up * 0.25f), (Vector2)negativeMag.transform.position + ((Vector2)negativeMag.transform.up * .25f) + ((Vector2)negativeMag.transform.up * -0.25f), Color.green, .5f);
                //Debug.DrawLine((Vector2)negativeMag.transform.position + ((Vector2)negativeMag.transform.up * .25f) + ((Vector2)negativeMag.transform.right * 0.25f), (Vector2)negativeMag.transform.position + ((Vector2)negativeMag.transform.up * .25f) + ((Vector2)negativeMag.transform.right * -0.25f), Color.green, .5f);
                #endregion

                // Positive
                if (magForwardSurface == null)
                {
                    foreach (Collider2D col in posForOverlap)
                    {
                        MagnetComponentBase curMag = col.gameObject.GetComponent<MagnetComponentBase>();
                        if (curMag != null && curMag._magData.charge == 1)
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
                                magForwardSurface = curMag.gameObject.GetComponent<Collider2D>();
                                break;
                            }
                        }  
                    }
                }

                // Negative
                if (magForwardSurface == null)
                {
                    foreach (Collider2D col in negForOverlap)
                    {
                        MagnetComponentBase curMag = col.gameObject.GetComponent<MagnetComponentBase>();
                        if (curMag != null && curMag._magData.charge == -1)
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
                                magForwardSurface = curMag.gameObject.GetComponent<Collider2D>();
                                break;
                            }
                        }    
                    }
                }

                if (magForwardSurface != null)
                {
                    Debug.Log("MagForwardSurf is " + magForwardSurface);
                }
                else
                {
                    Debug.Log("MagForwardSurf is null");
                }
                
                #endregion
            }
            else
            {
                #region Update evalAttractSurface
                Collider2D[] positiveOverlap = Physics2D.OverlapCircleAll((Vector2)positiveMag.transform.position + ((Vector2)positiveMag.transform.up * .05f), .5f, weldLayerMask);
                Collider2D[] negativeOverlap = Physics2D.OverlapCircleAll((Vector2)negativeMag.transform.position + ((Vector2)negativeMag.transform.up * .05f), .5f, weldLayerMask);

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
                #endregion
            }
            #endregion

            #region Prevent Fast Rotation Into Floors
            if (posPreventer.neutralDetected || negPreventer.neutralDetected || (evalAttractSurface != null && weldState == WeldState.None))
            {
                rb.angularDamping = 80f;
            }
            else
            {
                rb.angularDamping = Mathf.SmoothDamp(rb.angularDamping, .01f, ref angularRotVelocity, .1f);
            }
            #endregion

            #region Weld State Update Functionality
            if (weldState == WeldState.None)
            {
                // If is NOT touching a magnet or like within range of magnet.
                if ((evalAttractSurface == null && magForwardSurface == null)|| abilityProgression < 1)
                {
                    Debug.Log("Torque Rotation Method");
                    // Side Torque for stabilization
                    float currentAngle = rb.rotation;
                    float angleDifference = Mathf.DeltaAngle(currentAngle, Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg);
                    float torque = angleDifference;
                    rb.AddTorque(torque * Time.fixedDeltaTime);

                    // Drag
                    //rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, Mathf.Min(0f, rb.angularVelocity / 2f), Time.fixedDeltaTime * 2f);


                    // Correct Torque for aiming to software cursor
                    Vector3 dirA = transform.rotation * Vector3.up;
                    Vector3 dirB = targetAimQuat * Vector3.up;
                    float signedAngle = Vector3.SignedAngle(dirA, dirB, Vector3.forward);
                    float torqueAmount = (Mathf.Abs(signedAngle) <= 90f ? signedAngle : 90f * Mathf.Sign(signedAngle)) * rotationTorqueFactor;
                    rb.AddTorque(torqueAmount, ForceMode2D.Force);

                    // Damping
                    rb.AddTorque(-rb.angularVelocity * rotationTorqueDrag, ForceMode2D.Force);

                    // Continue updating intermediateRot so other move modes can transition well.
                    intermediateRot = transform.rotation;

                    //Debug.Log("Preventing fast rotation into floors 2");
                }
                else
                {
                    // If player is close to magnets neutrally, prevent player from pushing off against them via rigidbody. This is for game feel and avoiding large leaps caused by combined forces with repel.
                    if ((magForwardSurface != null || evalAttractSurface != null) && weldState == WeldState.None)
                    {
                        Debug.Log("Modify Transform Rotation Method");
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetAimQuat, Time.deltaTime * rotationFactor);
                        //rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetAimQuat, Time.deltaTime * rotationFactor));
                        intermediateRot = transform.rotation;
                    }
                    else
                    {
                        if (magForwardSurface != null)
                        {
                            Debug.Log("Mag forward surface..");
                        }
                        if (evalAttractSurface != null)
                        {
                            Debug.Log("eval attact surface..");
                        }
                        Debug.Log("Welded Rotation Method?");
                        // If on a neutral surface, use rigidbody for foddian movement or walk around.
                        if (neutralDetector.neutralDetected)
                        {
                            //Debug.Log("Neutral is detected!");
                            intermediateRot = Quaternion.Slerp(intermediateRot, targetAimQuat, Time.deltaTime * rotationFactor);
                            rb.MoveRotation(intermediateRot);
                        }
                        else
                        {
                            // Manually Update Transforms to face software cursor.
                            transform.rotation = Quaternion.Slerp(transform.rotation, targetAimQuat, Time.deltaTime * rotationFactor);
                            //intermediateRot = Quaternion.Slerp(transform.rotation, targetAimQuat, Time.deltaTime * rotationFactor);
                            //rb.MoveRotation(intermediateRot);
                            intermediateRot = transform.rotation;
                        }
                    } 
                }
            }
            else if (weldState == WeldState.Welded)
            {
                Vector3 weldAimDir = (playerWeldMag.transform.position - cursorObj.transform.position).normalized;

                Quaternion weldTargetAimQuat = Quaternion.LookRotation(Vector3.forward, weldAimDir);

                float angleFromNormal = Vector3.SignedAngle((Vector3)weldAimDir, (Vector3)weldSurfaceNormal, Vector3.forward);
     
                Vector3 clampVector = Quaternion.AngleAxis(weldAngleClamp * -1f * Mathf.Sign(angleFromNormal), Vector3.forward) * (Vector3)weldSurfaceNormal;

                if (playerWeldMag == positiveMag)
                {
                    clampVector = Vector3.Reflect(clampVector, (Vector3)weldSurfaceNormal);
                }

                Debug.DrawLine(playerWeldMag.transform.position, playerWeldMag.transform.position + ((Vector3)weldSurfaceNormal * 2f), Color.red, 7f);
                Debug.DrawLine(playerWeldMag.transform.position, playerWeldMag.transform.position + ((Vector3)weldAimDir * 2f), Color.green, 7f);
                Debug.DrawLine(playerWeldMag.transform.position, playerWeldMag.transform.position + (clampVector * 4f), Color.lightBlue, 7f);

                float totalAngle;
                float angle1;
                float angle2;
                if (playerWeldMag == negativeMag)
                {
                    totalAngle = Vector3.Angle(weldSurfaceNormal, clampVector);
                    angle1 = Vector3.Angle(weldAimDir, weldSurfaceNormal);
                    angle2 = Vector3.Angle(weldAimDir, clampVector);
                }
                else
                {
                    totalAngle = Vector3.Angle(-weldSurfaceNormal, clampVector);
                    angle1 = Vector3.Angle(weldAimDir, -weldSurfaceNormal);
                    angle2 = Vector3.Angle(weldAimDir, clampVector);
                }

                if (Mathf.Abs(angle1 + angle2) - Mathf.Abs(totalAngle) < .01f)
                {
                    Debug.Log("I am at an appropriate welding angle.");
                    intermediateRot = Quaternion.Slerp(intermediateRot, weldTargetAimQuat, Time.deltaTime * rotationFactor);
                    rb.MoveRotation(intermediateRot);
                }
                else
                {


                    Debug.Log("I am NOT at an appropriate welding angle. Correcting.");
                    Debug.Log(angle1 + " " + angle2 + " " + totalAngle);

                    //rb.MoveRotation(Quaternion.Euler(0, 0, rb.rotation) * Quaternion.AngleAxis(Vector3.SignedAngle((Vector3)weldAimDir, (Vector3)clampVector, Vector3.forward), Vector3.forward));
                    //intermediateRot = Quaternion.Euler(0, 0, rb.rotation);

                    //softwareCursor.SetLocalPos(clampVector * 10f);
                }

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
                    gameObject.GetComponent<BoxCollider2D>().size = new Vector2(FunctionLibraryF.MapRangeClamped(0.2f, 1f, playerColSize.x * .8f, playerColSize.x * .25f, softwareCursor.GetLaunchAlpha()), playerColSize.y * .8f);
                    gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(playerColOffset.x, ((playerWeldMag == positiveMag) ? -1f : 1f) * (playerColOffset.y + FunctionLibraryF.MapRangeClamped(0.2f, 1f, 0.2f, .22f, softwareCursor.GetLaunchAlpha())));

                    weldBlob.transform.position = playerWeldMag.transform.position + ((Vector3)weldSurfaceNormal * -.1f);
                    weldBlob.transform.rotation = Quaternion.LookRotation(weldBlob.transform.forward, weldSurfaceNormal);

                    // Tractor Beam VFX
                    posTractorBeamVFX.SetEndPos(positiveMag.transform.position);
                    negTractorBeamVFX.SetEndPos(negativeMag.transform.position);
                    posTractorBeamVFX.SetForceMagnitude(0f);
                    negTractorBeamVFX.SetForceMagnitude(0f);
                }
            #endregion

            if (abilityProgression > 0) // Does player have magnetism unlocked?
            {
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
                            float aimDirInfluenceAlpha;
                            if (evalAttractSurface == null)
                            {
                                aimDirInfluenceAlpha = FunctionLibraryF.MapRangeClamped(0f, 1f, 0f, 1f, Vector2.Dot(adjustedAimDir, curForce.normalized));
                                combinedPositiveForces += (Vector2.Dot(adjustedAimDir, curForce.normalized) * Vector2.ClampMagnitude((curForce.magnitude * Vector2.Lerp(curForce, (adjustedAimDir * curForce.magnitude), aimDirInfluenceAlpha).normalized), 80f));
                            }
                            else
                            {
                                aimDirInfluenceAlpha = FunctionLibraryF.MapRangeClamped(0f, 1f, 0f, 1f, Vector2.Dot(adjustedAimDir, curForce.normalized));
                                Vector2 nonFlippedForce = (Vector2.Dot(adjustedAimDir, curForce.normalized) * Vector2.ClampMagnitude((curForce.magnitude * Vector2.Lerp(curForce, (adjustedAimDir * curForce.magnitude), aimDirInfluenceAlpha).normalized), 80f));
                                nonFlippedForce *= FunctionLibraryF.MapRangeClamped(.5f, 1f, .3f, 1f, Vector2.Dot(adjustedAimDir, curForce.normalized));
                                combinedPositiveForces += nonFlippedForce;
                            }
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
                            float aimDirInfluenceAlpha;
                            if (evalAttractSurface == null)
                            {
                                //Debug.Log("Normal Pull");
                                aimDirInfluenceAlpha = FunctionLibraryF.MapRangeClamped(0f, 1f, 0f, 1f, Vector2.Dot(adjustedAimDir, curForce.normalized));
                                combinedNegativeForces += (Vector2.Dot(adjustedAimDir, curForce.normalized) * Vector2.ClampMagnitude((curForce.magnitude * Vector2.Lerp(curForce, (adjustedAimDir * curForce.magnitude), aimDirInfluenceAlpha).normalized), 80f));
                            }
                            else
                            {
                                //Debug.Log("Pull straight to wall");
                                aimDirInfluenceAlpha = FunctionLibraryF.MapRangeClamped(0f, 1f, 0f, 1f, Vector2.Dot(adjustedAimDir, curForce.normalized));
                                Vector2 nonFlippedForce = (Vector2.Dot(adjustedAimDir, curForce.normalized) * Vector2.ClampMagnitude((curForce.magnitude * Vector2.Lerp(curForce, (adjustedAimDir * curForce.magnitude), aimDirInfluenceAlpha).normalized), 80f));
                                nonFlippedForce *= FunctionLibraryF.MapRangeClamped(.5f, 1f, .3f, 1f, Vector2.Dot(adjustedAimDir, curForce.normalized));
                                combinedNegativeForces += nonFlippedForce;
                                //Debug.DrawLine((Vector2)negativeMag.transform.position, (Vector2)negativeMag.transform.position + (nonFlippedForce * 5f), Color.blue, 1f);
                            }  
                        }
                    }
                    #endregion

                    // Apply Forces

                    float velocityMultiplier = 1f;// FunctionLibraryF.MapRangeClamped(0f, 10f, .8f, 1.15f, velocity);
                    float angularMultiplier = 1f;//FunctionLibraryF.MapRangeClamped(0f, 25f, 1f, 1.25f, Mathf.Abs(angularVelocity));

                    // Positive
                    if (evalAttractSurface == null)
                    {
                        rb.AddForceAtPosition(combinedPositiveForces * velocityMultiplier * angularMultiplier, positiveMag.transform.position);
                    }
                    else
                    {
                        rb.AddForce(combinedPositiveForces * velocityMultiplier * angularMultiplier);
                    }
                    //Debug.DrawLine(positiveMag.transform.position, (Vector2)positiveMag.transform.position + (combinedPositiveForces * .25f));

                    #region Positive Beam VFX
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
                    #endregion

                    // Negative
                    if (evalAttractSurface == null)
                    {
                        rb.AddForceAtPosition(combinedNegativeForces * velocityMultiplier * angularMultiplier, negativeMag.transform.position);
                    }
                    else
                    {
                        rb.AddForce(combinedNegativeForces * velocityMultiplier * angularMultiplier);
                    }
                    //Debug.DrawLine(negativeMag.transform.position, (Vector2)negativeMag.transform.position + (combinedNegativeForces * .25f));

                    #region Negative Beam VFX
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
                    #endregion

                    #region Beam SFX
                    if (negativeFields.Count + positiveFields.Count > 0)
                    {
                        if (!sfxMagneticBeam.isPlaying)
                        {
                            sfxMagneticBeam.Play();
                        }

                        sfxMagneticBeam.volume = FunctionLibraryF.MapRangeClamped(0f, 30f, 0f, 1f, (combinedNegativeForces + combinedPositiveForces).magnitude) * FunctionLibraryF.MapRangeClamped(0f, 2f, .2f, 1f, velocity);
                    }
                    else
                    {
                        sfxMagneticBeam.Stop();
                    }
                    #endregion
                }
                #endregion
            }

            #region Scouting
            if (scoutState == ScoutState.Scouting)
            {
                scoutPosOffset = Vector2.SmoothDamp(scoutPosOffset, Vector2.ClampMagnitude(scoutPosOffset + (scoutInput * 9999999f * Time.fixedDeltaTime), scoutDistance), ref scoutVelocity, .3f);
            }
            else if (scoutState == ScoutState.Returning)
            {
                scoutPosOffset = Vector2.SmoothDamp(scoutPosOffset, Vector2.zero, ref scoutVelocity, .2f);
                //Debug.Log(scoutPosOffset);
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

            #region Grappling
            if (isGrappling)
            {
                grappleJoint.enabled = Vector2.Distance(transform.position, grapplePoint.position) >= grappleJoint.distance;

                float grappleRotBoost = FunctionLibraryF.MapRangeClamped(-30, 30, -2, 2, angularVelocity);
                Vector2 dirToGrapplePoint = (grapplePoint.position - transform.position).normalized;
                Vector2 grappleRotBoostDir = Quaternion.Euler(0, 0, (grappleRotBoost < 0f) ? -90f : 90f) * dirToGrapplePoint;
                rb.AddForce(Mathf.Abs(grappleRotBoost) * grappleRotBoostDir);

                if (Vector2.Dot(-dirToGrapplePoint, Vector2.down) > .6f)
                {
                    rb.AddForce(Quaternion.Euler(0, 0, (rb.linearVelocityX > 0f) ? -90f : 90f) * dirToGrapplePoint * .75f);
                    Debug.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, 0, (rb.linearVelocityX > 0f) ? -90f : 90f) * dirToGrapplePoint * 2f), Color.yellow, .5f);
                }
            }
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
    public void UpdateMouseDelta(InputAction.CallbackContext context)
    {
        if (inputMode == PlayerInputMode.Enabled)
        {
            mouseDelta = context.ReadValue<Vector2>().magnitude < 50f ? (context.ReadValue<Vector2>()) : FunctionLibraryF.ClampMagnitudeRange(context.ReadValue<Vector2>(), 50f, 0f);
        }
        else
        {
            mouseDelta = Vector2.zero;
        }
    }

    public void Scout(InputAction.CallbackContext context)
    {
        if (inputMode == PlayerInputMode.Enabled)
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
    }

    public void Weld(InputAction.CallbackContext context)
    {
        if (inputMode == PlayerInputMode.UIOnly)
        {
            if (context.started)
            {
                DialogueManager.instance.AdvanceDialogue();
            }
            return;
        }


        // Break out of spawn mechanism.
        if (AreaManager.instance.IsTransitionState(AreaManager.AreaTransitionState.Spawn))
        {
            AreaManager.instance.ReleasePlayer();
            ChangeWeldState(WeldState.None);
            weldInput = false;
        }

        else
        {
            // Does player have magnetism unlocked?
            if (abilityProgression > 0)
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
                if (battery.GetPercent() >= launchBatteryDrain)
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

    public void Interact(InputAction.CallbackContext context)
    {
        if (inputMode == PlayerInputMode.Enabled)
        {
            if (context.started)
            {
                interactObj.Interact(this);
            }
        }
            
    }
    
    public void LeftClick(InputAction.CallbackContext context)
    {
        if (inputMode != PlayerInputMode.Enabled)
        {
            return;
        }

        if (!context.started || isGrounded || isShootDisabled || !AreaManager.instance.IsTransitionState(AreaManager.AreaTransitionState.None))
        {
            return;
        }

        isShootDisabled = true;

        #region Burst Shot
        StartCoroutine(BurstShot());
        #endregion
    }

    private IEnumerator BurstShot()
    {
        float startVelMag = rb.linearVelocity.magnitude;
        int shotCount = 0;
        List<float> forceAmounts = new List<float>() { 125f, 50f, 100f };
        rb.linearDamping = .03f;

        while(shotCount < 3)
        {
            if (battery.GetPercent() > shotBatteryDrain)
            {
                battery.SubtractPercent(shotBatteryDrain);
                if (rb.linearVelocity.magnitude - startVelMag < 5f)
                {
                    projectilePool.ShootProjectile(negativeMag.transform.position, negativeMag.transform.rotation, new Vector3(.25f, .5f, .5f));
                    rb.AddForce(softwareCursor.GetAimDir() * forceAmounts[shotCount]);
                    sfxFerroProjectile.Play();
                }
                yield return new WaitForSeconds(.15f);
                shotCount++;
            }
            else
            {
                shotCount = 99999;
            }
        }

        rb.linearDamping = .01f;
        yield break;
    }

    public void RightClick(InputAction.CallbackContext context)
    {
        if (inputMode != PlayerInputMode.Enabled)
        {
            return;
        }

        if (context.started)
        {
            if (!isGrappling)
            {
                // Grapple has higher priority than bumpers. Bumpers will not happen if grapple does.
                #region Grapple
                List<Collider2D> grapplePoints = new List<Collider2D>();
                Collider2D[] grappleCircleOverlap = Physics2D.OverlapCircleAll(transform.position, grappleRange, (int)LayerMask.GetMask("Default"));
                foreach (Collider2D col in grappleCircleOverlap)
                {
                    if (col.CompareTag("Grapple"))
                    {
                        grapplePoints.Add(col);
                    }
                }

                if (grapplePoints.Count > 0)
                {
                    // Get the closest one.
                    Collider2D nearestGrapplePoint = (grapplePoints.OrderBy(c => (transform.position - c.transform.position).sqrMagnitude).ToList())[0];
                    grapplePoint = nearestGrapplePoint.transform;

                    // Update renderer
                    Transform[] lineTrans = new Transform[2];
                    lineTrans[0] = transform;
                    lineTrans[1] = grapplePoint;
                    grappleRenderer.SetPoints(lineTrans);
                    grappleRenderer.SetState(true);

                    // Update state
                    grappleJoint.distance = Mathf.Clamp(1.05f * Vector2.Distance(transform.position, grapplePoint.position), 1.5f, float.MaxValue);
                    grappleJoint.connectedAnchor = nearestGrapplePoint.transform.position;
                    //grappleJoint.enabled = true;

                    isGrappling = true;

                    return;
                }
                #endregion

                #region Bumpers
                Collider2D[] positiveEndOverlap = Physics2D.OverlapCircleAll((Vector2)positiveMag.transform.position + (Vector2)(positiveMag.transform.up * .2f), .5f, 1 << LayerMask.NameToLayer("Default"));
                Collider2D[] negativeEndOverlap = Physics2D.OverlapCircleAll((Vector2)negativeMag.transform.position + (Vector2)(negativeMag.transform.up * .2f), .5f, 1 << LayerMask.NameToLayer("Default"));

                int positiveOverlaps = 0;
                int negativeOverlaps = 0;

                #region Filter Results
                for (int i = 0; i < positiveEndOverlap.Length; i++)
                {
                    if (!positiveEndOverlap[i].gameObject.CompareTag("Player"))
                    {
                        positiveOverlaps++;
                    }
                    else
                    {
                        positiveEndOverlap[i] = null;
                    }
                }

                for (int i = 0; i < negativeEndOverlap.Length; i++)
                {
                    if (!negativeEndOverlap[i].gameObject.CompareTag("Player"))
                    {
                        negativeOverlaps++;
                    }
                    else
                    {
                        negativeEndOverlap[i] = null;
                    }
                }
                #endregion

                //Debug.Log(positiveOverlaps + negativeOverlaps);
                if (positiveOverlaps + negativeOverlaps > 0)
                {
                    if (positiveOverlaps > 0)
                    {
                        Vector2 dir = Vector2.zero;
                        Vector2[] dirs = new Vector2[positiveEndOverlap.Length];
                        for (int i = 0; i < positiveEndOverlap.Length; i++)
                        {
                            if (positiveEndOverlap[i] == null)
                            {
                                continue;
                            }

                            dirs[i] = ((Vector2)positiveMag.transform.position - positiveEndOverlap[i].ClosestPoint((Vector2)positiveMag.transform.position)).normalized;
                            dir += dirs[i];
                        }
                        dir /= dirs.Length;
                        rb.AddForceAtPosition(Vector2.Lerp(dir, -positiveMag.transform.up, 0.5f) * 700f, positiveMag.transform.position);
                    }

                    if (negativeOverlaps > 0)
                    {
                        Vector2 dir = Vector2.zero;
                        Vector2[] dirs = new Vector2[negativeEndOverlap.Length];
                        for (int i = 0; i < negativeEndOverlap.Length; i++)
                        {
                            if (negativeEndOverlap[i] == null)
                            {
                                continue;
                            }
                            //Debug.Log(negativeEndOverlap[i]);
                            dirs[i] = ((Vector2)negativeMag.transform.position - negativeEndOverlap[i].ClosestPoint((Vector2)negativeMag.transform.position)).normalized;
                            //Debug.DrawLine(negativeMag.transform.position, negativeMag.transform.position + ((Vector3)dirs[i] * 5f), Color.green, 1f);
                            dir += dirs[i];
                        }
                        dir /= dirs.Length;
                        rb.AddForceAtPosition(Vector2.Lerp(dir, -negativeMag.transform.up, 0.5f) * 700f, negativeMag.transform.position);
                        //Debug.DrawLine(negativeMag.transform.position, negativeMag.transform.position + ((Vector3)dir * 5f), Color.red, 1f);
                    }
                }
                #endregion
            }

        }
        else if (context.canceled)
        {
            if (isGrappling)
            {
                grappleRenderer.SetState(false);
                grappleJoint.enabled = false;
                isGrappling = false;
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
            gameObject.GetComponent<BoxCollider2D>().offset = playerColOffset;
            gameObject.GetComponent<BoxCollider2D>().size = playerColSize;

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
            gameObject.GetComponent<BoxCollider2D>().offset = playerColOffset;
            gameObject.GetComponent<BoxCollider2D>().size = playerColSize;

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
                rb.constraints = RigidbodyConstraints2D.None;
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
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                UpdateScalePivotTransforms();
                gameObject.transform.parent = scalePivot.transform;
                // Set anchor data for the hingejoint
                gameObject.GetComponent<HingeJoint2D>().enabled = true;

                // Adjust capsule collider.
                gameObject.GetComponent<BoxCollider2D>().offset = playerColOffset + new Vector2(0f, ((playerWeldMag == positiveMag) ? -.15f : .15f));
                gameObject.GetComponent<BoxCollider2D>().size = playerColOffset + new Vector2(playerColSize.x * .8f, playerColSize.y * .8f);

                weldBlob.transform.position = playerWeldMag.transform.position + ((Vector3)weldSurfaceNormal * -.1f);
                weldBlob.transform.rotation = Quaternion.LookRotation(weldBlob.transform.forward, weldSurfaceNormal);
                weldBlob.SetActive(true);

                // Update camera follow target.
                CameraManager.instance.AddFollowTarget(playerWeldMag.transform, 0.5f);
                CameraManager.instance.RemoveFollowTarget(gameObject.transform, 0.5f);

                break;
            case WeldState.LaunchAim:

                // Adjust capsule collider.
                gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(playerColOffset.x, ((playerWeldMag == positiveMag) ? -1f : 1f) * (playerColOffset.y + FunctionLibraryF.MapRangeClamped(0.2f, 1f, 0.2f, .22f, softwareCursor.GetLaunchAlpha())));
                gameObject.GetComponent<BoxCollider2D>().size = new Vector2(FunctionLibraryF.MapRangeClamped(0.2f, 1f, playerColSize.x * .8f, playerColSize.x * .25f, softwareCursor.GetLaunchAlpha()), playerColSize.y * .8f);

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

        battery.SubtractPercent(FunctionLibraryF.MapRangeClamped(.25f, 1f, 4f, launchBatteryDrain, softwareCursor.GetLaunchAlpha()));

        float launchForce = Mathf.Pow(launchBaseConstant, FunctionLibraryF.MapRangeClamped(0f, 1f, 1.1f, launchMaxExponent, softwareCursor.GetLaunchAlpha()));
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
        rb.linearVelocity = new Vector2(0f, 0f);
        rb.gravityScale = 0f;
        rb.WakeUp();
    }

    public void RestartInput(InputAction.CallbackContext context)
    {
        if (inputMode != PlayerInputMode.Enabled)
        {
            return;
        }

        if (context.performed) /// If held for duration specified in the Input Settings.
        {
            Restart();
        }       
    }
    public void Restart()
    {
        projectilePool.ClearPool();
        if (AreaManager.instance.roomManager.doesResetFullyReloadLevel)
        {
            AreaManager.instance.ReloadCurrentRoom();
            battery.SetPercent(GameInstance.instance.roomStartBattery);
        }
        else
        {
            transform.position = startPos;
            rb.linearVelocity = Vector3.zero;
            battery.SetPercent(GameInstance.instance.roomStartBattery);
        } 
    }

    public void BeginDeath(DamageTypes damageType)
    {
        if (!isDead)
        {
            isDead = true;
            StartCoroutine(DeathAnimation(damageType));
        }
        
    }
    private IEnumerator DeathAnimation(DamageTypes damageType)
    {
        rb.linearVelocity = Vector2.zero;
        inputMode = PlayerInputMode.Disabled;
        yield return new WaitForSeconds(.75f);
        inputMode = PlayerInputMode.Enabled;
        Death();
        yield break;
    }
    public void Death()
    {
        // TO DO: Ensure reset room state when respawning in the same room.
        // TO DO: Reset battery percentage to be what is was upon entering the room.
        // TO DO: Checkpoint logic
        isDead = false;
        switch(GameInstance.instance.difficulty)
        {
            case GameInstance.GameDifficulty.Easy:
                if (SceneManagement.GetSceneFormattedName(AreaManager.instance.GetCurrentRoom()) == "a1_r6")
                {
                    if (GameInstance.instance.playerLives <= 0)
                    {
                        AreaManager.instance.Respawn();
                    }
                    else
                    {
                        Restart();
                    }
                }
                else
                {
                    Restart();
                }  
                break;

            case GameInstance.GameDifficulty.Normal:
                if (GameInstance.instance.playerLives <= 0)
                {
                    AreaManager.instance.Respawn();
                }
                else
                {
                    GameInstance.instance.SetPlayerLives((byte)(GameInstance.instance.playerLives - 1));
                    Restart();
                }
                break;

            case GameInstance.GameDifficulty.Hardcore:
                break;
            default:
                break;
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

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("MagnetSurface"))
        {
            sfxSurfaceMagnet.volume = FunctionLibraryF.MapRangeClamped(0f, 10f, 0f, 3f, velocity);
            sfxSurfaceMagnet.Play();
        }
        else
        {
            sfxSurfaceGeneric.volume = FunctionLibraryF.MapRangeClamped(0f, 10f, 0f, 3f, velocity);
            sfxSurfaceGeneric.Play();
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

    #region IDamageable
    public void Damage(DamageTypes damageType)
    {
        BeginDeath(damageType);
    }

    public bool IsAffectedByDamageType(DamageTypes damageType)
    {
        return true;
    }
    #endregion

    #region Debug
    public void DebugSkipToNextRoom()   // Remove this in builds.
    {
        if (Application.isEditor)
        {
            AreaManager.instance.ReachedObjective(GameObject.FindAnyObjectByType<roomObjective>().gameObject);
        }
    }

    #region Ability Progression
    public void ProgressAbility(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ProgressAbility();
        }
    }

    public void ProgressAbility()
    {
        if (abilityProgression < 255)
        {
            abilityProgression++;
            GameInstance.instance.UpdatePlayerAbilityProgression(abilityProgression);
            spriteManager.UpdateSpriteByProgression(abilityProgression);
        }
    }
    public void RevertAbility(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            RevertAbility();
        }
    }

    public void RevertAbility()
    {
        if (abilityProgression > 0)
        {
            abilityProgression--;
            GameInstance.instance.UpdatePlayerAbilityProgression(abilityProgression);
            spriteManager.UpdateSpriteByProgression(abilityProgression);
        }
    }
    #endregion
    #endregion
}
