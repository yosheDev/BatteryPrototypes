using UnityEngine;

namespace Magnet
{
    public interface IMagnetic
    {
        MagnetData GetMagData();
        bool AffectsRadius(Vector2 posWS, float radius); /// Affects radius

        Vector2 GetNearestPoint(); /// Returns nearest point of magnet surface. Used for distance calculations in physics forces.
        Vector2 GetAppliedForce(MagnetData magData, Vector2 posWS, float radius); /// Gets applied force from MagneticPoints.
    }
}
