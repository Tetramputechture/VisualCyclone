using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;


namespace VisualCycloneGUI.Cyclone
{
    public class CyclonePath
    {
        public string ID { get; set; }

        public LineSeries path { get; }

        private IList<CyclonePoint> Points { get; }

        public CyclonePath(string ID)
        {
            this.ID = ID;
            Points = new List<CyclonePoint>();
            path = new LineSeries
            {
                MarkerFill = OxyColors.Transparent,
                MarkerType = MarkerType.Circle,
                MarkerSize = 0,
                ItemsSource = Points,
                TrackerFormatString =
                        "Date: {Date:yyyy-MM-dd HH:mm}\nLatitude: {4:0.0}{LatitudeDirection}\nLongitude: {2:0.0}{LongitudeDirection}\nID: " + ID
            };
        }

        public void AddPoint(CyclonePoint p)
        {
            Points.Add(p);
        }
    }
}
