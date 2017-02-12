namespace GreatCIrcle.Domain.Model
{
    /// <summary>
    /// Way point class. Represents a point along a route.
    /// </summary>
    public class WayPoint
    {
        /// <summary>
        /// Creates a new WayPoint instance.
        /// </summary>
        public WayPoint() { }

        /// <summary>
        /// Creates a new WayPoint instance.
        /// </summary>
        /// <param name="lat">The latitude of the way point.</param>
        /// <param name="lng">The longitude of the way point.</param>
        public WayPoint(double lat, double lng) { Latitude = lat; Longitude = lng; }

        /// <summary>
        /// The latitude of the way point.
        /// </summary>
        public double Latitude { get; set; } = double.NaN;

        /// <summary>
        /// The longitude of the way point.
        /// </summary>
        public double Longitude { get; set; } = double.NaN;
    }
}
