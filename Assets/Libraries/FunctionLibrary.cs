using UnityEngine; /// Include so nice functions are avaliable.

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
    }
}