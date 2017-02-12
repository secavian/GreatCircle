using System;
using GreatCIrcle.Domain.Extensions;

namespace GreatCIrcle.Domain.Helpers
{
    /// <summary>
    /// Angular conversions
    /// </summary>
    public static class Conversions
    {
        /// <summary>
        /// Converts angles in degrees to radians. Returns NaN if value is NaN.
        /// </summary>
        /// <param name="degrees">The angle in degrees to convert to radians.</param>
        /// <returns>NaN if degrees is NaN; otherwise, returns radians value.</returns>
        public static double DegreesToRadians(this double degrees)
        {
            if (degrees.IsNaN()) { return degrees; }
            return degrees * Math.PI / 180;
        }

        /// <summary>
        /// Converts angles in radians to degrees. Returns NaN if value is NaN.
        /// </summary>
        /// <param name="radians">The angle in radians to convert to degrees.</param>
        /// <returns>NaN if radians is NaN; otherwise, returns degrees value.</returns>
        public static double RadiansToDegrees(this double radians)
        {
            if (radians.IsNaN()) { return radians; }
            var degrees = radians * 180 / Math.PI;
            while (degrees > 359)
            {
                degrees -= 360;
            }
            return degrees;
        }
    }
}
