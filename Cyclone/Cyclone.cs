using System;

namespace VisualCyclone.Cyclone
{

    // alias <int, direction> tuple as Geographic Coordinate for ease of use
    using GeoCoord = Tuple<int, Direction>;

    public class Cyclone
    {
        // the date associated with this cyclone
        public DateTime Date { get; }

        // the latidude of the cyclone. Direction must be North or South.
        public GeoCoord Latitude { get; }

        // the longitude of the cyclone. Direction must be East or West.
        public GeoCoord Longitude { get; }


        // constructs a new Cyclone. 
        // date must be formatted as YYYYMMDDHH.
        // latitudeDirection must be North or South.
        // longitudeDirection must be East or West.
        public Cyclone(string date, int latitudeValue, Direction latitudeDirection, int longitudeValue,
            Direction longitudeDirection)
        {
            // arguments check
            if (date.Length != 10)
            {
                throw new ArgumentException("date must be formatted as YYYYMMDDHH.");
            }

            if (latitudeDirection != Direction.North && latitudeDirection != Direction.South)
            {
                throw new ArgumentException("latitudeDirection must be Direction.North or Direction.South");
            }

            if (longitudeDirection != Direction.East && longitudeDirection != Direction.West)
            {
                throw new ArgumentException("longitudeDirection must be Direction.East or direction.West");
            }

            Date = new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(4, 2)),
                int.Parse(date.Substring(6, 2)), int.Parse(date.Substring(8, 2)), 0, 0);

            Latitude = new GeoCoord(latitudeValue, latitudeDirection);
            Longitude = new GeoCoord(longitudeValue, longitudeDirection);
        }
    }
}
