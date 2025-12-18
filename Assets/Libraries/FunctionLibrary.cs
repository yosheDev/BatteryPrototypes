using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
/// Include so nice functions are avaliable.

// Magnet Data
namespace FunctionLibrary
{
    public static class FunctionLibraryF
    {
        public static float MapRangeClamped(float input_min, float input_max, float output_min, float output_max, float value)
        {
            if (value < input_min)
            {
                return output_min;
            }
            else if (value > input_max)
            {
                return output_max;
            }
            else
            {
                return (value - input_min) / (input_max - input_min) * (output_max - output_min) + output_min;
            }
        }

        public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position)
        {
            var worldToLocalMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
            return worldToLocalMatrix.MultiplyPoint3x4(position);
        }
        public static bool IsInLayerMask(GameObject obj, LayerMask mask) => (mask.value & (1 << obj.layer)) != 0;
        public static bool IsInLayerMask(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;
    }
}