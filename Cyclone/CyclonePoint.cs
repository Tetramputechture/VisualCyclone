using System;
using OxyPlot;

namespace VisualCycloneGUI.Cyclone
{
    /*
     * Provides info about a cyclone. Currently stores date and location information.
     * Used by OxyPlot to graph the point, specified by GetDataPoint
    */
    public class CyclonePoint : IDataPointProvider
    {
        /*
         * the date associated with this cyclone
        */
        public DateTime Date { get; set; }

        /*
         * the latidude value of the cyclone
        */
        public float LatitudeValue { get; set; }

        /*
         * the latidude direction of the cyclone. must be N or S
        */
        public string LatitudeDirection { get; set; }

        /* 
         * the longitude value of the cyclone
        */
        public float LongitudeValue { get; set; }

        /*
         * the longitude direction of the cyclone. must be E or W
        */
        public string LongitudeDirection { get; set; }

        public CyclonePoint(string date, float latitudeValue, string latitudeDirection, float longitudeValue,
            string longitudeDirection)
        {
            Date = Convert.ToDateTime(date);

            LatitudeValue = latitudeValue;
            LatitudeDirection = latitudeDirection;
            LongitudeValue = longitudeValue;
            LongitudeDirection = longitudeDirection;
        }

        public override string ToString()
        {
            return
                $"Date: {Date}\nLatitude: {LatitudeValue}{LatitudeDirection}\nLongitude: {LongitudeValue}{LongitudeDirection}";
        }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(LongitudeValue, LatitudeValue);
        }
    }
}
