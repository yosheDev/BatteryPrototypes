using Magnet;
using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

// This class is to be the parent of all magnet component scripts. This makes finding all magnetic objects much easier and efficient.
// Marked abstract, meaning can not be instantiated. Children derived of this class can be.
public abstract class MagnetComponentBase : MonoBehaviour, IMagnetic
{
    //=======================================================================================================================
    [Header("Magnet Data")]
    public MagnetData _magData = new MagnetData();
    protected const float _magFactor = 100f;/// This is a constant factor that is multiplied with all calculated forces. This will affect all magnets project-wide.

    [HideInInspector]public HashSet<MagnetComponentBase> affectFields = new HashSet<MagnetComponentBase>();
    //=======================================================================================================================

    // ===[ IMagnetic Functions ]===========================
    public MagnetData GetMagData()
    {
        return GetMagDataOverride();
    }
    public abstract MagnetData GetMagDataOverride();

    public Vector2 GetNearestPoint()
    {
        return GetNearestPointOverride();
    }
    public abstract Vector2 GetNearestPointOverride();

    public bool AffectsRadius(Vector2 posWS, float radius)
    {
        return AffectsRadiusOverride(posWS, radius);
    }
    public abstract bool AffectsRadiusOverride(Vector2 posWS, float radius);

    // Multiple overrides for the different types of magnet shapes?
    public Vector2 GetAppliedForce(MagnetData magData, Vector2 posWS, float radius) /// Point
    {
        return GetAppliedForceOverride(magData, posWS, radius);
    }
    public abstract Vector2 GetAppliedForceOverride(MagnetData magData, Vector2 posWS, float radius);
}
