using UnityEngine; /// Include so nice functions are avaliable.

// Magnet Data
namespace Magnet
{
    [System.Serializable]
    public struct MagnetData
    {
        public float charge;          /// Is either -1 or 1.
        public float strength;        /// Strength of magnet.
        public float proportionConst; /// Material property (may not use this one.)


        public MagnetData(float inCharge = 1f, float inStrength = 1f, float inProportionalConst = 1f)
        {
            charge = Mathf.Clamp(inCharge, -1f, 1f);
            strength = inStrength;
            proportionConst = inProportionalConst;
        }
    };
}
