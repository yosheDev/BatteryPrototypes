using UnityEngine;
using Magnet;

// Magnetic Point is derived from MagnetComponentBase. It uses a single point as reference for the field(currently, might change?)
public class MagneticSurface : MagnetComponentBase
{
    [Header("Surface Data (Collider types must match.)")]
    [Tooltip("Collider used for overlapping field operations.")]
    public Collider2D fieldCol;
    [Tooltip("Collider of the hard-surface collision. If null, uses as a point in space for distance calculations. If valid, uses perimeter of collider for distance calculations.")]
    public Collider2D surfaceCol;
    [Tooltip("Distance for attraction from closest collider perimeter point.")]

    [Header("Field Attraction Data")]
    // TO DO: Add toggle to switche between different attuentuation curves. I.E Exponential, Logarithmic, Linear.

    public float _fieldAttractDistance = 2f;
    [Tooltip("Exponential attenuation factor for forces. Stronger as it gets closer to the surface of the magnet.")]
    public float _attenuation = 2f;

    
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
            else if (surfaceCol is PolygonCollider2D)
            {
                // PolygonCollider2Ds will not be able to have the collider automatically adjust as easily as it is for box and circle. This should
                // probably just be done manually per-instance for better control. (Especially since this is a rapid-prototype too.)
            }
        }
        #endregion
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

    public override Vector2 GetAppliedForceOverride(MagnetData magData, Vector2 posWS, float radius) /// Parent interface function for IMagnetic calls this abstract function.
    {
        // _magData = this magnets data.
        // magData = input magnets data.

        #region Calculate Force

        // Point of this magnet nearest to the other magnet.
        Vector2 nearestPoint = GetNearestPoint(posWS);

        #region Occlusion Test
        // TO DO: At the moment, values under 1f do nothing. It is either occludes or does not. No transmission affecting values is possible at the moment.
        RaycastHit2D[] hits = Physics2D.LinecastAll(posWS, nearestPoint, Physics2D.DefaultRaycastLayers);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject.GetComponent<FieldOccluder>())
            {
                float occlusion = hit.collider.gameObject.GetComponent<FieldOccluder>().occlusion;
                if (occlusion >= 1f)
                {
                    return Vector2.zero;
                }
                break;
            }
        }
        #endregion

        // Multiply strength by charge for each.
        float mag1Amp = magData.strength * magData.charge;
        float mag2Amp = _magData.strength * _magData.charge;

        // Multiply that result together, then multiply by proportionConst
        float force = mag1Amp * mag2Amp;

        // Divide by distance^2
        force /= Mathf.Pow(Vector2.Distance(posWS, nearestPoint), _attenuation);

        // Get force direction(normalized) and multiply with force.
        Vector2 result = (force * (posWS - nearestPoint).normalized) * _magFactor;

        #endregion

        return result;
    }
}
