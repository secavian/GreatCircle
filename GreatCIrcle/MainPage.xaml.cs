using System;
using System.Collections.Generic;
using GreatCIrcle.Domain.Helpers;
using GreatCIrcle.Domain.Model;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;

namespace GreatCIrcle
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //arbitrary - the larger the number, the fewer the mid points for a leg. too large and the line doesn't look like a curve.
        //too small, and the amount of calculations grows. tweak as needed
        const int GREAT_CIRCLE_SEGMENT_LENGTHS_KILOMETERS = 250;

        //hold the various points on the route as we create them.
        private List<WayPoint> _waypoints = new List<WayPoint>();

        public MainPage()
        {
            InitializeComponent();
        }

        //clears the map for a fresh run
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _waypoints.Clear();
            UpdateMap();
        }

        //responds to holding map with left button or long press
        //adds a way point at the selected position and updates the map
        private void Map_MapHolding(MapControl sender, MapInputEventArgs args)
        {
            var latitude = args.Location.Position.Latitude;
            var longitude = args.Location.Position.Longitude;

            _waypoints.Add(new WayPoint(latitude, longitude));

            UpdateMap();
        }

        //clears the map then draws the new route. called each time a new point is added
        private void UpdateMap()
        {
            //remove existing way point and line segments on map.
            Map.MapElements.Clear();

            //draw a new set of lines
            DrawLinesOnMap();

            //identify the way points
            AddPushPinsToMap();
        }

        //draws route lines using Great Circle Navigation math
        //https://en.wikipedia.org/wiki/Great-circle_navigation
        private void DrawLinesOnMap()
        {
            if (_waypoints.Count > 1)
            {
                //******************************************************************
                //call into the method that does the heavy lifting for calculations.
                //******************************************************************
                List<BasicGeoposition> geopositions = GetGeopositions();

                //probably unnecessary, but we'll check anyway
                if (geopositions.Count == 0) { return; }

                //define the lines (which is actually 1 or more line segments)
                var path = new Geopath(geopositions);
                var line = new MapPolyline();
                line.StrokeThickness = 5;
                line.StrokeColor = Colors.CornflowerBlue;
                line.Path = path;

                //and put them on the map.
                Map.MapElements.Add(line);
            }
        }

        //iterates the way points. if there are 2 or more, calculates the line segments needed to
        //draw a geodesic arc between each way point pair
        private List<BasicGeoposition> GetGeopositions()
        {
            //list to hold the various points that make up the great circle
            var geopositions = new List<BasicGeoposition>();

            WayPoint previousWaypoint = null;

            foreach (var waypoint in _waypoints)
            {
                //if this is not the first way point along the route, calculate the sub-points that define the
                //great circle arc we're creating
                if (previousWaypoint != null)
                {
                    var srcLatDeg = previousWaypoint.Latitude;
                    var srcLngDeg = previousWaypoint.Longitude;
                    var tarLatDeg = waypoint.Latitude;
                    var trgLngDeg = waypoint.Longitude;

                    //holds the individual points that we're using to make up the arc.
                    //this method returns a pre-populated collection of way points which
                    //we'll update next
                    int segmentLength;
                    var midpoints = SetupMidPointsList(previousWaypoint, waypoint, out segmentLength);

                    //if the leg distance is too short to bother with, the list won't have any thing to hold.
                    //however, it there is a count > 0, then we need to get the points that make up our great arc
                    if(midpoints.Count > 0)
                    {
                        //get the line segments between the two points
                        CalcGreatCircleMidPoints(previousWaypoint, waypoint, 0, midpoints.Count, midpoints, segmentLength);

                        //add them to the list.
                        foreach (var m in midpoints)
                        {
                            geopositions.Add(m);
                        }
                    }
                }

                //add the end point of the given leg to the line. if this is the first way point in the list, it is added
                //as the start point.
                geopositions.Add(new BasicGeoposition() { Latitude = waypoint.Latitude, Longitude = waypoint.Longitude });

                previousWaypoint = waypoint;
            }

            return geopositions;
        }

        //this approach defines a list already populated with the needed points. the points are not very
        //useful at this point but that will be handled later
        private List<BasicGeoposition> SetupMidPointsList(WayPoint previousWaypoint, WayPoint waypoint, out int segmentLength)
        {
            segmentLength = GREAT_CIRCLE_SEGMENT_LENGTHS_KILOMETERS;

            //how far apart are the two way points?
            var distanceBetweenPoints =
                Calculations.GreatCircleDistance(previousWaypoint.Latitude, previousWaypoint.Longitude, waypoint.Latitude, waypoint.Longitude);

            //return an empty list if the distance is below our threshold
            var midpoints = new List<BasicGeoposition>();
            if (distanceBetweenPoints <= GREAT_CIRCLE_SEGMENT_LENGTHS_KILOMETERS)
            {
                return midpoints;
            }

            //otherwise figure out how many points we're going to need (one point per segment)
            int numberOfMidPoints = 0;

            //not sure how -right- this is, but if there is only one mid point, the calcs go crazy,
            //so we're assuming the distance is close enough to display as a straight line.
            //otherwise, make sure the number of points is an even number; not sure why we
            //need that, but it seems necessary
            while (true)
            {
                numberOfMidPoints = (int)(distanceBetweenPoints / segmentLength);
                if (numberOfMidPoints == 1) { return midpoints; }

                if (numberOfMidPoints % 2 != 0)
                {
                    segmentLength -= 25;
                }
                else
                {
                    break;
                }
            }

            //pre-populate the list with placeholder values
            for (var i = 0; i <= numberOfMidPoints; i++)
            {
                midpoints.Add(new BasicGeoposition());
            }

            return midpoints;
        }

        //recursive function. calculates the mid point of a given line segment and if the distance isn't too great,
        //adds the mid point to the line being drawn if the segment is too long, splits the line in two and repeats
        //the same process for each half line segment. repeats until all the individual line segments are short
        //enough to render a curve
        private void CalcGreatCircleMidPoints(WayPoint startPoint, WayPoint endPoint, int lowerIndex, int upperIndex, List<BasicGeoposition> points, int segmentLength)
        {
            //how far apart are the two points?
            var distance = Calculations.GreatCircleDistance(startPoint.Latitude, startPoint.Longitude, endPoint.Latitude, endPoint.Longitude);

            //once we've iterated to the point where the points don't exceed our threshold, we're done.
            if (distance <= segmentLength) { return; }

            //find the point between the two midpoints
            var midPoint = Calculations.GreatCircleMidPoint(startPoint.Latitude,startPoint.Longitude, endPoint.Latitude, endPoint.Longitude);
            var geopoint = new BasicGeoposition() { Latitude = midPoint.Position.Latitude, Longitude = midPoint.Position.Longitude };

            //find the index it should be inserted at and add it
            var index = (lowerIndex + upperIndex) / 2;
            points[index] = geopoint;

            //split the two way points in half and repeat for the lower and upper halves
            var wp = new WayPoint(geopoint.Latitude, geopoint.Longitude);
            CalcGreatCircleMidPoints(startPoint, wp, lowerIndex, index, points, segmentLength);
            CalcGreatCircleMidPoints(wp, endPoint, index, upperIndex, points, segmentLength);
        }

        //puts a push pin icon on the map at each way point to let the user know something happened.
        private void AddPushPinsToMap()
        {
            WayPoint previousWayPoint = null;
            foreach (var waypoint in _waypoints)
            {
                var lat = waypoint.Latitude;
                var lng = waypoint.Longitude;

                AddPushPinToMap(lat, lng);

                previousWayPoint = waypoint;
            }
        }

        private void AddPushPinToMap(double lat, double lng)
        {
            var pushpin = new MapIcon();
            pushpin.Location = new Geopoint(new BasicGeoposition() { Latitude = lat, Longitude = lng });
            pushpin.Title = $"{lat:0.00} | {lng:0.00}";

            pushpin.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Pin.png"));

            pushpin.CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible;
            pushpin.NormalizedAnchorPoint = new Point(0.5, 0.5);
            Map.MapElements.Add(pushpin);
        }
    }
}
