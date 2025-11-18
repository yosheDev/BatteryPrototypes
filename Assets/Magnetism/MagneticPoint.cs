using UnityEngine;
using Magnet;

// Magnetic Point is derived from MagnetComponentBase. It uses a single point as reference for the field(currently, might change?)
public class MagneticPoint : MagnetComponentBase
{
    [Header("Point Data")]
    public float _radius = 2f;          /// Radiuse for attraction.
    public float _attenuation = 2f;     /// Attentuation factor for force.

    private Vector2 GetForce(Vector2 otherPosWS, Vector2 otherVelocity, MagnetData otherMagnet)
    {
        Vector2 output = new Vector2();

        // Get direction of force.


        // Magnetism Strength Formula
        // Notes: This needs to account for velocity. Formula I found must have been for something else.
        //float distance = Vector2.Distance((Vector2)transform.position, otherPosWS);
        //float strengthNum = _magData.strength * otherMagnet.strength;
        //float attraction = _magData.proportionConst * (strengthNum / (distance * distance));



        return output;
    }

    // ===[ IMagnetic Functions ]===========================
    public override MagnetData GetMagDataOverride() /// Parent interface function for IMagnetic calls this abstract function.
    {
        return _magData;
    }

    public override Vector2 GetNearestPointOverride()
    {
        return transform.position;
    }
    public override bool AffectsRadiusOverride(Vector2 posWS, float radius) /// Parent interface function for IMagnetic calls this abstract function.
    {
        ///float centerPointDistance = Vector2.Distance(transform.position, posWS);
        ///float radiiSum = radius + _radius;

        // If the radii of the two points overlap with each other.
        return (Vector2.Distance(transform.position, posWS) < (radius + _radius));
    }

    public override Vector2 GetAppliedForceOverride(MagnetData magData, Vector2 posWS, float radius) /// Parent interface function for IMagnetic calls this abstract function.
    {
        // TO DO: Implement
        //magData.strength
        //magData.charge
        //magData.proportionConst

        // _magData = this magnets data.
        // magData = input magnets data.

        // Point of this magnet nearest to the other magnet.
        Vector2 nearestPoint = GetNearestPoint();

        // Multiply strength by charge for each.
        float mag1Amp = magData.strength * magData.charge;
        float mag2Amp = _magData.strength * _magData.charge;

        // Multiply that result together, then multiply by proportionConst
        float force = mag1Amp * mag2Amp;

        // Divide by distance^2
        force /= Mathf.Pow(Vector2.Distance(posWS, nearestPoint), 2f);

        // Get force direction(normalized) and multiply with force.
        Vector2 result = (force * (nearestPoint - posWS).normalized) * _magFactor;

        return result;
    }
}
