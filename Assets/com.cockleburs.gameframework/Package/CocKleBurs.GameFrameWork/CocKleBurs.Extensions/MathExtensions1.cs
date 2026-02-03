
using UnityEngine;

namespace CockleBurs.GameFramework.Extension
{
    /// <summary>
    ///     Includes additional maths methods
    /// </summary>
    public static partial class MathExtensions
    {
        /// <summary>
        ///     Returns negative-one, zero, or postive-one of a value instead of just negative-one or positive-one.
        /// </summary>
        /// <param name="value">Value to sign.</param>
        /// <returns>Precise sign.</returns>
        public static float PreciseSign(this float value)
        {
            return value == 0f ? 0f : Mathf.Sign(value);
        }
    }
}