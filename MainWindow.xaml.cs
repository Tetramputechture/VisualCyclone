using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using VisualCycloneGUI.Cyclone;

namespace VisualCycloneGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            Points = new List<CyclonePoint>();
            Annotations = new List<Annotation>();
            DataPlot = new PlotModel { Title = "Cyclone Frequency" };

            _lineseries = new LineSeries
            {
                Color = OxyColors.Transparent,
                MarkerFill = OxyColors.SteelBlue,
                MarkerType = MarkerType.Circle,
                ItemsSource = Points,
                TrackerFormatString = "Date: {Date:yyyy-MM-dd HH:mm}\n{1}: {2:0.0}{LongitudeDirection}\n{3}: {4:0.0}{LatitudeDirection}"
            };

            DataPlot.Series.Add(_lineseries);

            _lonAxis = new LinearAxis()
            {
                Title = "Longitude",
                Position = AxisPosition.Bottom
            };

            DataPlot.Axes.Add(_lonAxis);

            _latAxis = new LinearAxis()
            {
                Title = "Latitude",
                Position = AxisPosition.Left
            };

            DataPlot.Axes.Add(_latAxis);

            // build database if not already built
            DataFormatter.DataFormatter.CreateDatabase(true);

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;

            // todo: make these bounds dependent on data. e.g. if data only goes from 1966 to 1970, display only those years
            for (var i = 1945; i <= 2015; i++)
            {
                StartYearComboBox.Items.Add(i);
                EndYearComboBox.Items.Add(i);
            }
            StartYearComboBox.SelectedItem = 1945;
            EndYearComboBox.SelectedItem = 2015;
        }

        public void LowerLatBoundChanged(object sender, TextChangedEventArgs e)
        {
            _lowerLatBound = float.Parse(LowerLatitudeTextBox.Text == "" ? "0" : LowerLatitudeTextBox.Text);
            _latAxis.Minimum = _lowerLatBound;
            UpdateQuery();
        }

        public void UpperLatBoundChanged(object sender, TextChangedEventArgs e)
        {
            _upperLatBound = float.Parse(UpperLatitudeTextBox.Text == "" ? "0" : UpperLatitudeTextBox.Text);
            _latAxis.Maximum = _upperLatBound;
            UpdateQuery();
        }

        public void LowerLonBoundChanged(object sender, TextChangedEventArgs e)
        {
            _lowerLonBound = float.Parse(LowerLongitudeTextBox.Text == "" ? "0" : LowerLongitudeTextBox.Text);
            _lonAxis.Minimum = _lowerLonBound;
            UpdateQuery();
        }

        public void UpperLonBoundChanged(object sender, TextChangedEventArgs e)
        {
            _upperLonBound = float.Parse(UpperLongitudeTextBox.Text == "" ? "0" : UpperLongitudeTextBox.Text);
            _lonAxis.Maximum = _upperLonBound;
            UpdateQuery();
        }

        public void StartYearBoundChanged(object sender, SelectionChangedEventArgs e)
        {
            _startYearBound = StartYearComboBox.SelectionBoxItem.ToString();
            UpdateQuery();
        }

        public void EndYearBoundChanged(object sender, SelectionChangedEventArgs e)
        {
            _endYearBound = EndYearComboBox.SelectionBoxItem.ToString();
            UpdateQuery();
        }

        private void UpdateQuery()
        {
            Points.Clear();
            // todo: make date query work correctly...
            CycloneDatabase.DbConnection.Open();
            var sql = "select distinct * from cyclones " +
                      $"WHERE latitudeValue BETWEEN {_lowerLatBound} and {_upperLatBound} " +
                      "AND latitudeDirection = 'S' " +
                      $"AND longitudeValue BETWEEN {_lowerLonBound} AND {_upperLonBound} " +
                      "AND longitudeDirection = 'W' ";
                      //$"AND date > '01/01/{_startYearBound}' AND date < '01/01/{_endYearBound}'";
            var command = new SQLiteCommand(sql, CycloneDatabase.DbConnection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var date = reader["date"].ToString();

                var lonPoint = float.Parse(reader["longitudeValue"].ToString());
                lonPoint = (float)Math.Round(lonPoint, 1, MidpointRounding.AwayFromZero);

                var lonDir = reader["longitudeDirection"].ToString();

                var latPoint = float.Parse(reader["latitudeValue"].ToString());
                latPoint = (float)Math.Round(latPoint, 1, MidpointRounding.AwayFromZero);

                var latDir = reader["latitudeDirection"].ToString();

                Points.Add(new CyclonePoint(date, latPoint, latDir, lonPoint, lonDir));
            }
            CycloneDatabase.DbConnection.Close();
            DataPlot.InvalidatePlot(true);
        }
    
        public void CheckLatitudeInput(object sender, TextCompositionEventArgs e) => e.Handled = !IsLatitudeAllowed(((TextBox)sender).Text + e.Text);

        public void CheckLongitudeInput(object sender, TextCompositionEventArgs e) => e.Handled = !IsLongitudeAllowed(((TextBox)sender).Text + e.Text);

        private bool IsLatitudeAllowed(string str)
        {
            decimal dummy;
            if (!(decimal.TryParse(str, out dummy))) return false;

            var latVal = float.Parse(str);
            return latVal >= 0 && latVal <= 90;
        }

        private bool IsLongitudeAllowed(string str)
        {
            decimal dummy;
            if (!(decimal.TryParse(str, out dummy))) return false;

            var lonVal = float.Parse(str);
            return lonVal >= 0 && lonVal <= 180;
        }

        private float _lowerLatBound;
        private float _upperLatBound;
        private float _lowerLonBound;
        private float _upperLonBound;

        private string _startYearBound;
        private string _endYearBound;

        private LineSeries _lineseries;
        private LinearAxis _lonAxis;
        private LinearAxis _latAxis;

        public PlotModel DataPlot { get; private set; }

        public IList<CyclonePoint> Points { get; private set; }
        public IList<Annotation> Annotations { get; private set; }
    }
}
