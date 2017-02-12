using static System.Math;
using System;
using GreatCIrcle.Domain.Extensions;
using Windows.Devices.Geolocation;

namespace GreatCIrcle.Domain.Helpers
{
    public class Calculations
    {
        public const double EARTH_RADIUS_KILOMETERS = 6371.0;

        /// <summary>
        /// Gets the geodesic distance in kilometers between two points on a globe.
        /// </summary>
        /// <param name="srcLatDeg">Latitude of starting point in degrees.</param>
        /// <param name="srcLngDeg">Longitude of ending point in degrees.</param>
        /// <param name="trgLatDeg">Latitude of starting point in degrees.</param>
        /// <param name="trgLngDeg">Longitude of ending point in degrees.</param>
        /// <returns>Distance between two points in kilometers</returns>
        /// <remarks>https://en.wikipedia.org/wiki/Haversine_formula</remarks>
        public static double GreatCircleDistance(double srcLatDeg, double srcLngDeg, double trgLatDeg, double trgLngDeg)
        {
            var deltaLatDeg = trgLatDeg - srcLatDeg;
            var deltaLatRad = Conversions.DegreesToRadians(deltaLatDeg);

            var deltaLngDeg = trgLngDeg - srcLngDeg;
            var deltaLngRad = Conversions.DegreesToRadians(deltaLngDeg);

            var srcLatRad = Conversions.DegreesToRadians(srcLatDeg);
            var trgLatRad = Conversions.DegreesToRadians(trgLatDeg);

            var haversine = 
                HaverSin(deltaLatRad) + 
                (
                    Cos(srcLatRad) * Cos(trgLatRad) * HaverSin(deltaLngRad)
                );

            var d = 2.0 * EARTH_RADIUS_KILOMETERS * Asin(Sqrt(haversine));

            return d;
        }

        /// <summary>
        /// Returns the mid point of a great circle line segment.
        /// </summary>
        /// <param name="srcLatDeg">Latitude of starting point in degrees.</param>
        /// <param name="srcLngDeg">Longitude of ending point in degrees.</param>
        /// <param name="trgLatDeg">Latitude of starting point in degrees.</param>
        /// <param name="trgLngDeg">Longitude of ending point in degrees.</param>
        /// <returns>Midpoint between two points as Geo point (values in degrees)</returns>
        /// <remarks>http://www.movable-type.co.uk/scripts/latlong.html</remarks>
        public static Geopoint GreatCircleMidPoint(double srcLatDeg, double srcLngDeg, double trgLatDeg, double trgLngDeg)
        {
            var srcLatRad = Conversions.DegreesToRadians(srcLatDeg);
            var srcLngRad = Conversions.DegreesToRadians(srcLngDeg);
            var trgLatRad = Conversions.DegreesToRadians(trgLatDeg);
            var deltaLngDeg = trgLngDeg - srcLngDeg;
            var deltaLngRad = Conversions.DegreesToRadians(deltaLngDeg);

            var Bx = Cos(trgLatRad) * Cos(deltaLngRad);
            var By = Cos(trgLatRad) * Sin(deltaLngRad);
            var latMid = 
                Atan2
                (
                    Sin(srcLatRad) + Sin(trgLatRad), 
                    Sqrt(Pow(Cos(srcLatRad) + Bx, 2) + Pow(By, 2))
                );

            var lngMid = srcLngRad + Atan2(By, Cos(srcLatRad) + Bx);

            return new Geopoint(new BasicGeoposition() { Latitude = latMid.RadiansToDegrees(), Longitude = lngMid.RadiansToDegrees() });
        }

        /// <summary>
        /// Calculates an average between two double. Treats NaN as 0.
        /// </summary>
        /// <param name="a">First value</param>
        /// <param name="b">Second value</param>
        /// <returns>Average of two values</returns>
        private static double Average(double a, double b)
        {
            if (a.IsNaN()) { a = 0; }
            if (b.IsNaN()) { b = 0; }

            return (a + b) / 2;
        }

        /// <summary>
        /// Calculates the "half versed sine" of an angle in radians.
        /// </summary>
        /// <param name="angle">The angle to base the value on in radians.</param>
        /// <returns>The "half versed sine" of an angle in radians.</returns>
        /// <remarks>
        /// https://en.wikipedia.org/wiki/Haversine_formula
        /// https://en.wikipedia.org/wiki/Versine#hav
        /// </remarks>
        public static double HaverSin(double angle) => Pow(Sin(angle) / 2, 2); // ~= (1 - Cos(angle)) / 2
    }
}
