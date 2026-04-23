using Magnet;
using System.Collections.Generic;
using UnityEngine;

// This class is to be the parent of all magnet component scripts. This makes finding all magnetic objects much easier and efficient.
// Marked abstract, meaning can not be instantiated. Children derived of this class can be.
public abstract class MagnetComponentBase : MonoBehaviour, IMagnetic
{
    //=======================================================================================================================
    [Header("Magnet Data")]
    public MagnetData _magData = new MagnetData(1f, 1f, 2f);
    protected const float _magFactor = 20f;/// This is a constant factor that is multiplied with all calculated forces. This will affect all magnets project-wide.

    [HideInInspector]public HashSet<MagnetComponentBase> affectFields = new HashSet<MagnetComponentBase>();
    //=======================================================================================================================

    #region IMagnetic Functions
    public MagnetData GetMagData()
    {
        return GetMagDataOverride();
    }
    public void SetMagData(MagnetData newMagData)
    {
        _magData = newMagData;
    }

    public void ReversePolarity()
    {
        MagnetData newMagData = _magData;
        newMagData.charge *= -1f;
        _magData = newMagData;
    }

    public abstract MagnetData GetMagDataOverride();

    public Vector2 GetNearestPoint(Vector2 posWS) /// Returns nearest point on surface of magnets area. Used to get proper distance value that is not always reliant on pivot point.
    {
        return GetNearestPointOverride(posWS);
    }
    public abstract Vector2 GetNearestPointOverride(Vector2 posWS);

    public bool AffectsRadius(Vector2 posWS, float radius)
    {
        return AffectsRadiusOverride(posWS, radius);
    }
    public abstract bool AffectsRadiusOverride(Vector2 posWS, float radius);

    // Multiple overrides for the different types of magnet shapes?
    public Vector2 GetAppliedForce(MagnetData magData, Vector2 posWS, float radius, Vector2 velocity)
    {
        return GetAppliedForceOverride(magData, posWS, radius, velocity);
    }
    public abstract Vector2 GetAppliedForceOverride(MagnetData magData, Vector2 posWS, float radius, Vector2 velocity);
    #endregion
}
