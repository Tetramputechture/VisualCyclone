using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using VisualCycloneGUI.Cyclone;

namespace VisualCycloneGUI
{
    public partial class MainWindow
    {
        /* 
         * The CycloneDatabase our program will work with.
        */
        private readonly CycloneDatabase _cycloneDatabase;

        /* 
         * Latitude and longitude bounds.
        */
        private float _lowerLatBound;
        private float _upperLatBound;
        private float _lowerLonBound;
        private float _upperLonBound;

        /*
         * Years listed in year selection boxes.
        */
        public IList<int> AcceptableYears { get; }

        /* 
         * Year bounds.
        */
        private string _startYearBound;
        private string _endYearBound;

        /*
         * Latitude and longitude axes.
        */
        private readonly LinearAxis _lonAxis;
        private readonly LinearAxis _latAxis;

        /* 
         * The plot where the data is displayed.
        */
        public PlotModel DataPlot { get; }

        /*
         * The CyclonePoints the DataPlot will read and display.
        */
        public IList<CyclonePoint> Points { get; }

        /*
         * Executes when the main window is created.
        */

        public MainWindow()
        {
            // Initialize members
            Points = new List<CyclonePoint>();
            DataPlot = new PlotModel {Title = "Cyclone Frequency"};

            // This defines how the data is to be plotted.
            // Right now it's blue dots with a tooltip showing year and exact location.
            // todo: extend functionality to include more ways of data visualization
            var lineseries = new LineSeries
            {
                Color = OxyColors.Transparent,
                MarkerFill = OxyColors.SteelBlue,
                MarkerType = MarkerType.Circle,
                ItemsSource = Points,
                TrackerFormatString =
                    "Date: {Date:yyyy-MM-dd HH:mm}\n{1}: {2:0.0}{LongitudeDirection}\n{3}: {4:0.0}{LatitudeDirection}"
            };

            DataPlot.Series.Add(lineseries);

            // Create data axes and add them to the plot
            _lonAxis = new LinearAxis
            {
                Title = "Longitude",
                Position = AxisPosition.Bottom
            };

            DataPlot.Axes.Add(_lonAxis);

            _latAxis = new LinearAxis
            {
                Title = "Latitude",
                Position = AxisPosition.Left
            };

            DataPlot.Axes.Add(_latAxis);

            // Add acceptable years to be displayed year selection boxes. 
            // todo: make these bounds dependent on data. e.g. if data only goes from 1966 to 1970, display only those years
            AcceptableYears = new List<int>();
            for (var i = 1945; i <= 2015; i++)
            {
                AcceptableYears.Add(i);
            }

            // build database if not already built
            _cycloneDatabase = new CycloneDatabase("cycloneDB.sqlite");
            if (!_cycloneDatabase.IsExistingDb)
            {
                Console.WriteLine($"Building Database {_cycloneDatabase.DbFileName}...\n");
                _cycloneDatabase.InsertDataFromDirectory("RawCycloneData");
                Console.WriteLine("Database built.");
            }

            // Initialize main window components defined in the MainWindow.xaml file
            InitializeComponent();
        }

        /*
         * Executes when the main window is loaded.
        */
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // set the datacontext to this mainwindow object so we can reference objects in the aassociated xaml 
            DataContext = this;

            // assign preselected dates
            StartYearComboBox.SelectedItem = 1945;
            EndYearComboBox.SelectedItem = 2015;
        }

        /*
         * Function that executes when the lower latitude bound is changed.
        */
        public void LowerLatBoundChanged(object sender, TextChangedEventArgs e)
        {
            _lowerLatBound = float.Parse(string.IsNullOrWhiteSpace(LowerLatitudeTextBox.Text) ? "0" : LowerLatitudeTextBox.Text);
            _latAxis.Minimum = _lowerLatBound;
            UpdateQuery();
        }

        /*
         * Function that executes when the upper latitude bound is changed.
        */
        public void UpperLatBoundChanged(object sender, TextChangedEventArgs e)
        {
            _upperLatBound = float.Parse(string.IsNullOrWhiteSpace(UpperLatitudeTextBox.Text) ? "0" : UpperLatitudeTextBox.Text);
            _latAxis.Maximum = _upperLatBound;
            UpdateQuery();
        }

        /*
         * Function that executes when the lower longitude bound is changed.
        */
        public void LowerLonBoundChanged(object sender, TextChangedEventArgs e)
        {
            _lowerLonBound = float.Parse(string.IsNullOrWhiteSpace(LowerLongitudeTextBox.Text) ? "0" : LowerLongitudeTextBox.Text);
            _lonAxis.Minimum = _lowerLonBound;
            UpdateQuery();
        }

        /*
         * Function that executes when the upper longitude bound is changed.
        */
        public void UpperLonBoundChanged(object sender, TextChangedEventArgs e)
        {
            _upperLonBound = float.Parse(string.IsNullOrWhiteSpace(UpperLongitudeTextBox.Text) ? "0" : UpperLongitudeTextBox.Text);
            _lonAxis.Maximum = _upperLonBound;
            UpdateQuery();
        }

        /*
         * Function that executes when the start year bound is changed.
        */
        public void StartYearBoundChanged(object sender, SelectionChangedEventArgs e)
        {
            _startYearBound = StartYearComboBox.SelectedItem.ToString();
            UpdateQuery();
        }

        /*
         * Function that executes when the lower year bound is changed.
        */
        public void EndYearBoundChanged(object sender, SelectionChangedEventArgs e)
        {
            _endYearBound = EndYearComboBox.SelectedItem.ToString();
            UpdateQuery();
        }

        /* 
         * Updates the current database query and 
         * adds the queried points to the list of points to be displayed.
        */
        private void UpdateQuery()
        {
            // clear the old points
            Points.Clear();

            // open the database connection for writing
            _cycloneDatabase.DbConnection.Open();

            // make the query based on selected bounds
            // todo: implement direction queries
            // user must be able to specify queries like 90S to 90N, 180E to 180W
            var sql = "select distinct * from cyclones " +
                      $"WHERE latitudeValue BETWEEN {_lowerLatBound} and {_upperLatBound} " +
                      "AND latitudeDirection = 'S' " +
                      $"AND longitudeValue BETWEEN {_lowerLonBound} AND {_upperLonBound} " +
                      "AND longitudeDirection = 'W' " +
                      $"AND date BETWEEN date('{_startYearBound}-01-01') AND date('{_endYearBound}-12-31') ";

            // make sql query for the database out of the command string
            var command = new SQLiteCommand(sql, _cycloneDatabase.DbConnection);

            // read the points accpeted by the query
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                // get date
                var date = reader["date"].ToString();

                // get longitude value and round it to nearest tenth
                var lonPoint = float.Parse(reader["longitudeValue"].ToString());
                lonPoint = (float) Math.Round(lonPoint, 1, MidpointRounding.AwayFromZero);

                // get longitude direction
                var lonDir = reader["longitudeDirection"].ToString();

                // get latitude value and round it to nearest tenth
                var latPoint = float.Parse(reader["latitudeValue"].ToString());
                latPoint = (float) Math.Round(latPoint, 1, MidpointRounding.AwayFromZero);

                // get latitude direction
                var latDir = reader["latitudeDirection"].ToString();

                // add new CyclonePoint to list of points to be displayed
                Points.Add(new CyclonePoint(date, latPoint, latDir, lonPoint, lonDir));
            }

            // we are done reading from the database. close the connection
            _cycloneDatabase.DbConnection.Close();

            // force plot to update
            DataPlot.InvalidatePlot(true);
        }

        // checks latitude and longitude value inputs
        // todo: make specific checks for lower and upper bounds. can't have lower bound be greater than upper bound etc
        public void CheckLatitudeInput(object sender, TextCompositionEventArgs e)
            => e.Handled = !IsLatitudeAllowed(((TextBox) sender).Text + e.Text);

        public void CheckLongitudeInput(object sender, TextCompositionEventArgs e)
            => e.Handled = !IsLongitudeAllowed(((TextBox) sender).Text + e.Text);

        private bool IsLatitudeAllowed(string str)
        {
            decimal dummy;
            if (!(decimal.TryParse(str, out dummy))) return false;

            var latVal = dummy;
            return latVal >= 0 && latVal <= 90;
        }

        private bool IsLongitudeAllowed(string str)
        {
            decimal dummy;
            if (!(decimal.TryParse(str, out dummy))) return false;

            var lonVal = dummy;
            return lonVal >= 0 && lonVal <= 180;
        }
    }
}
