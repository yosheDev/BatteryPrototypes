using FunctionLibrary;
using Magnet;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Magnetic Point is derived from MagnetComponentBase. It uses a single point as reference for the field(currently, might change?)
public class MagneticSurface : MagnetComponentBase
{
    [Header("Surface Data (Collider types must match.)")]
    [Tooltip("Collider used for overlapping field operations.")]
    public Collider2D fieldCol;
    [Tooltip("Collider of the hard-surface collision. If null, uses as a point in space for distance calculations. If valid, uses perimeter of collider for distance calculations.")]
    public Collider2D surfaceCol;

    [Header("Field Attraction Data")]
    // TO DO: Add toggle to switche between different attuentuation curves. I.E Exponential, Logarithmic, Linear.
    public float _fieldAttractDistance = 2f;
    [Tooltip("Multiplies with the attenuation in magData. Used for customizing feel of individual magnets. This applies only to the force given to those interacting with this magnet.")]
    public float _attenuationModifier = 1f;

    [Header("Player")]
    [Tooltip("Adjusts influence player magnets will have on this.")]
    public float _playerMagInfluence = 2f;
    [Tooltip("Can this magnet push and pull the player rigidbody?")]
    public bool _canAffectPlayer = true;
    private bool isOnPlayer = false;    /// Is this magnet a player magnet?

    private Rigidbody2D rb;
    [Header("Rigidbody Only Properties")]

    [Tooltip("Clamps any forces added by this maximum threshold.")]
    public float _forceClamp = 2f;
    [Tooltip("The maximum velocity that this is allowed to move.")]
    public float _maxVelocity = 2f;
    [Tooltip("The maximum angular velocity that this is allowed to move.")]
    public float _maxAngularVelocity = 10f;


    // VFX
    [Header("VFX")]
    public bool bakeToMagFieldSDF = true;
    private MagneticDistanceFieldsManager sdfManager;

    private void OnValidate()
    {
        #region Coordinate Field Collider, Surface Collider, and Surface Data Properties.
        if (surfaceCol != null)
        {
            if (surfaceCol is CircleCollider2D)
            {
                if (fieldCol is CircleCollider2D)
                {
                    ((CircleCollider2D)fieldCol).radius = _fieldAttractDistance + (surfaceCol != null ? ((CircleCollider2D)surfaceCol).radius : 0);
                }
                else
                {
                    throw new System.Exception("Field Collider is not a CircleCollider2D, but SurfaceCollider is.");
                }
            }
            else if (surfaceCol is BoxCollider2D)
            {
                if (fieldCol is BoxCollider2D)
                {
                    ((BoxCollider2D)fieldCol).size = ((BoxCollider2D)surfaceCol).size;
                    ((BoxCollider2D)fieldCol).edgeRadius = _fieldAttractDistance;
                }
                else
                {
                    throw new System.Exception("Field Collider is not a BoxCollider2D, but SurfaceCollider is.");
                }
            }
            else if(surfaceCol is CapsuleCollider2D)
            {
                if (fieldCol is CapsuleCollider2D)
                {
                    ((CapsuleCollider2D)fieldCol).size = new Vector2((((CapsuleCollider2D)surfaceCol).size.x + _fieldAttractDistance) * .66f, ((CapsuleCollider2D)surfaceCol).size.y + _fieldAttractDistance);
                }
                else
                {
                    throw new System.Exception("Field Collider is not a CapsuleCollider2D, but SurfaceCollider is.");
                }
            }
            else if (surfaceCol is EdgeCollider2D)
            {
                if (fieldCol is EdgeCollider2D)
                {
                    ((EdgeCollider2D)fieldCol).points = ((EdgeCollider2D)surfaceCol).points;
                    ((EdgeCollider2D)fieldCol).edgeRadius = _fieldAttractDistance;
                }
            }
            else if (surfaceCol is PolygonCollider2D)
            {
                // PolygonCollider2Ds will not be able to have the collider automatically adjust as easily as it is for box and circle. This should
                // probably just be done manually per-instance for better control. (Especially since this is a rapid-prototype too.)
            }
        }
        #endregion
    }

    private void Awake()
    {
        // Pass surface collider onto the SDF Manager.
        sdfManager = FindAnyObjectByType<MagneticDistanceFieldsManager>(); /// Only ever one of these at a time.
        if (sdfManager != null)
        {
            if (surfaceCol != null && bakeToMagFieldSDF == true)
            {
                sdfManager.AddFieldCollider(surfaceCol);
            }
        }

        isOnPlayer = gameObject.CompareTag("Player");
    }

    private void Start()
    {
        if (GetComponent<Rigidbody2D>() != null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }
    private void FixedUpdate()
    {
        if (rb != null && !isOnPlayer)
        {
            List<MagnetComponentBase> otherFields = affectFields.ToList();
            //Debug.Log("Affect Fields: " + affectFields.Count + " | Other Fields: " + otherFields.Count);
            Vector2 totalForce = Vector2.zero;
            for (int i = 0; i < otherFields.Count; i++)
            {
                totalForce += ((otherFields[i].gameObject.CompareTag("Player")) ? _playerMagInfluence : 1f) * otherFields[i].GetAppliedForce(_magData, transform.position, _fieldAttractDistance, rb.linearVelocity);
            }

            Debug.DrawLine(transform.position, transform.position + (Vector3)(totalForce.normalized * 1f), Color.red, .1f);
            rb.AddForce(Vector2.ClampMagnitude(totalForce, _forceClamp));

            rb.linearVelocity = FunctionLibraryF.ClampMagnitudeRange(rb.linearVelocity, _maxVelocity, 0f);
            rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, 0f, _maxAngularVelocity);
        }    
    }

    // ===[ IMagnetic Functions ]===========================
    public override MagnetData GetMagDataOverride() /// Parent interface function for IMagnetic calls this abstract function.
    {
        return _magData;
    }

    public override Vector2 GetNearestPointOverride(Vector2 posWS)
    {
        // If magnet acts as literal point.
        if (surfaceCol == null)
        {
            return transform.position;
        }
        else // If magnet has surface area. 
        {
            return surfaceCol.ClosestPoint(posWS);
        }
    }
    public override bool AffectsRadiusOverride(Vector2 posWS, float radius) /// Parent interface function for IMagnetic calls this abstract function.
    {
        // TO DO: Make this work for all collider types.
        // Not sure what this function will potentially be used for in the future. Now that colliders have been added, probably use those for collision checks instead?

        // If the radii of the two points overlap with each other.
        return (Vector2.Distance(transform.position, posWS) < (radius + _fieldAttractDistance));
    }

    public override Vector2 GetAppliedForceOverride(MagnetData magData, Vector2 posWS, float radius, Vector2 velocity) /// Parent interface function for IMagnetic calls this abstract function.
    {
        if (isOnPlayer)
        {
            // Dot product check.
            if (Vector2.Dot((posWS - (Vector2)transform.position).normalized, transform.up) <= .5f)
            {
                return Vector2.zero;
            }
        }
        // _magData = this magnets data.
        // magData = input magnets data.

        #region Calculate Force

        // Point of this magnet nearest to the other magnet.
        Vector2 nearestPoint = GetNearestPoint(posWS);

        #region Occlusion Test
        float occlusion = 1f;
        //// TO DO: At the moment, values under 1f do nothing. It is either occludes or does not. No transmission affecting force values is possible at the moment.
        //RaycastHit2D[] hits = Physics2D.LinecastAll(posWS, nearestPoint, Physics2D.DefaultRaycastLayers);
        RaycastHit2D[] hits = Physics2D.CircleCastAll(posWS, .3f, (nearestPoint - posWS).normalized, Vector2.Distance(posWS, nearestPoint), Physics2D.DefaultRaycastLayers);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject.GetComponent<FieldOccluder>())
            {
                occlusion = Mathf.Clamp(hit.collider.gameObject.GetComponent<FieldOccluder>().occlusion, 0f, 1f);
                occlusion = 1f - occlusion;
                break;
            }
        }
        #endregion

        #region Magnetism Force
        // 1 - Get correct charge.
        float combinedAmp = magData.charge * _magData.charge;
        //if(Vector2.Distance(posWS, nearestPoint) < .5f)
        //{
        //    Debug.Log(Vector2.Distance(posWS, nearestPoint));
        //}
        // 2 - Inverse Exponent Formula. Ensure distance to always be above zero(also padding for game feel). Gets maximum attenuation of both magDatas to act with. Ensure attenuation is not under 1.
        float proximityPadding = Mathf.Clamp(.3f, .01f, float.MaxValue); /// Adjust to affect too large of forces when magnet is nearly touching the other magnet.
        float force = combinedAmp * (1 / (Mathf.Pow(Mathf.Max(Vector2.Distance(posWS, nearestPoint), proximityPadding), (Mathf.Max(1f, Mathf.Max(_magData.attenuation, magData.attenuation)) * _attenuationModifier))));

        // 3 - Modify force magnitude. Account for magnets' strength. Account for target magnet's velocity.
        /// Get direction of the magnetic field.
        Vector2 magForceDir = (posWS - nearestPoint).normalized;
        /// Get magnitude of magnetic force (magnetic force = cross(velocity, magnetic field)). Prevent from being under 1 at all times. Clamp max value for game feel.
        float crossMagnitude = Mathf.Clamp((velocity.x * magForceDir.y) - (velocity.y * magForceDir.x), 1f, 3f);
        /// Calculate magnitude of force to act on other magnet.
        force *= magData.strength * _magData.strength * crossMagnitude;

        // 4 - Create resulting vector2 force. Account for global magnet strength factor. Account for calculated occlusion scalar.
        Vector2 result = ((force * magForceDir) * _magFactor) * occlusion;

        //Debug.Log("Cross Magnitude: " + crossMagnitude);
        //Debug.Log("Force: " + force + " | Result: " + result);
        #endregion

        #endregion
        return result;
    }
}
