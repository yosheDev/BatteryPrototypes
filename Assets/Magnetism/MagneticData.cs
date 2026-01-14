using UnityEngine; /// Include so nice functions are avaliable.

// Magnet Data
namespace Magnet
{
    [System.Serializable]
    public struct MagnetData
    {
        public float charge;          /// Is either -1 or 1.
        public float strength;        /// Strength of magnet.
        public float attenuation;     /// This is the constant used in inverse power. Dipoles have 3 by default, everything else has 2.


        public MagnetData(float inCharge = 1f, float inStrength = 1f, float inAttenuation = 2f)
        {
            charge = Mathf.Clamp(inCharge, -1f, 1f);
            strength = inStrength;
            attenuation = inAttenuation;
        }
    };
}
