using UnityEditor.U2D.Aseprite;
using UnityEngine;

namespace Magnet
{
    public interface IMagnetic
    {
        MagnetData GetMagData();

        void SetMagData(MagnetData newMagData);
        void ReversePolarity();
        bool AffectsRadius(Vector2 posWS, float radius); /// Affects radius
        Vector2 GetNearestPoint(Vector2 posWS); /// Returns nearest point of magnet surface. Used for distance calculations in physics forces.
        Vector2 GetAppliedForce(MagnetData magData, Vector2 posWS, float radius, Vector2 velocity); /// Gets applied force from MagneticPoints.
    }
}
