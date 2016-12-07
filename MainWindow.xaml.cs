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
         * The minimum year of storms within the cyclone database.
        */
        private int _minYear { get; set; }

        /*
         * The maximum year of storms within the cyclone database.
        */
        private int _maxYear { get; set; }

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

        public PlotModel PathPlot { get; }

        /*
         * The CyclonePoints the DataPlot will read and display.
        */
        public IList<CyclonePoint> Points { get; }

        private LineSeries pathSeries { get; }

        /*
         * Executes when the main window is created.
        */

        public MainWindow()
        {
            PathPlot = new PlotModel { Title = "Storm Paths" };

            // Create data axes and add them to the plot
            _lonAxis = new LinearAxis
            {
                Title = "Longitude (°E)",
                Position = AxisPosition.Bottom,
                AbsoluteMinimum = -180,
                AbsoluteMaximum = 180,
                Minimum = -180,
                Maximum = -160,
                MinimumMinorStep = 0.5,
                MinimumMajorStep = 0.5
            };

            _lonAxis.AxisChanged += GraphZoomed;

            //DataPlot.Axes.Add(_lonAxis);
            PathPlot.Axes.Add(_lonAxis);

            _latAxis = new LinearAxis
            {
                Title = "Latitude (°N)",
                Position = AxisPosition.Left,
                AbsoluteMinimum = -90,
                AbsoluteMaximum = 90,
                Minimum = -20,
                Maximum = 0,
                MinimumMinorStep = 0.5,
                MinimumMajorStep = 0.5
            };

            _latAxis.AxisChanged += GraphZoomed;

            //DataPlot.Axes.Add(_latAxis);
            PathPlot.Axes.Add(_latAxis);

            // build database if not already built
            _cycloneDatabase = new CycloneDatabase("cycloneDBFinal.sqlite");
            if (!_cycloneDatabase.IsExistingDb)
            {
                Console.WriteLine($"Building Database {_cycloneDatabase.DbFileName}...\n");
                _cycloneDatabase.InsertDataFromDirectory("RawCycloneData");
                Console.WriteLine("Database built.");
            }
            GetMinMaxYears();

            // Add acceptable years to be displayed year selection boxes. 
            // todo: make these bounds dependent on data. e.g. if data only goes from 1966 to 1970, display only those years
            AcceptableYears = new List<int>();
            for (var i = _minYear; i <= _maxYear; i++)
            {
                AcceptableYears.Add(i);
            }

            // Initialize main window components defined in the MainWindow.xaml file
            InitializeComponent();
        }

        private void GetMinMaxYears()
        {
            // open the database connection for writing
            _cycloneDatabase.DbConnection.Open();

            // make the query based on selected bounds
            var sql = "select max(date), min(date) from cyclones";

            // make sql query for the database out of the command string
            var command = new SQLiteCommand(sql, _cycloneDatabase.DbConnection);

            // read the points accpeted by the query
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                _maxYear = int.Parse(reader[0].ToString().Substring(0, 4));
                _minYear = int.Parse(reader[1].ToString().Substring(0, 4));
            }

            // we are done reading from the database. close the connection
            _cycloneDatabase.DbConnection.Close();
        }

        /*
         * Executes when the main window is loaded.
        */
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // set the datacontext to this mainwindow object so we can reference objects in the aassociated xaml 
            DataContext = this;

            // assign preselected dates
            StartYearComboBox.SelectedItem = _minYear;
            EndYearComboBox.SelectedItem = _maxYear;
        }

        /*
         * Function that executes when the lower latitude bound is changed.
        */
        public void LowerLatBoundChanged(object sender, TextChangedEventArgs e)
        {
            var val = float.Parse(string.IsNullOrWhiteSpace(LowerLatitudeTextBox.Text) ? "0" : LowerLatitudeTextBox.Text);

            if (LowerLatitudeDirection != null && LowerLatitudeDirection.SelectedValue.ToString() == "S")
            {
                val = -val;
            }

            _latAxis.Minimum = val;
            UpdateQuery();
        }

        /*
         * Function that executes when the lower latitude direction is changed.
        */
        public void LowerLatDirChanged(object sender, SelectionChangedEventArgs e)
        {
            var val = float.Parse(string.IsNullOrWhiteSpace(LowerLatitudeTextBox.Text) ? "0" : LowerLatitudeTextBox.Text);
            if (LowerLatitudeDirection.SelectedValue == null || LowerLatitudeDirection.SelectedValue.ToString() == "S")
            {
                val = -val;
            }
            _latAxis.Minimum = val;
            UpdateQuery();
        }

        /*
         * Function that executes when the upper latitude bound is changed.
        */
        public void UpperLatBoundChanged(object sender, TextChangedEventArgs e)
        {
            var val = float.Parse(string.IsNullOrWhiteSpace(UpperLatitudeTextBox.Text) ? "0" : UpperLatitudeTextBox.Text);
            if (UpperLatitudeDirection != null && UpperLatitudeDirection.SelectedValue.ToString() == "S")
            {
                val = -val;
            }

            _latAxis.Maximum = val;
            UpdateQuery();
        }

        /*
         * Function that executes when the upper latitude direction is changed.
        */
        public void UpperLatDirChanged(object sender, SelectionChangedEventArgs e)
        {
            var val = float.Parse(string.IsNullOrWhiteSpace(UpperLatitudeTextBox.Text) ? "0" : UpperLatitudeTextBox.Text);
            if (UpperLatitudeDirection.SelectedValue != null && UpperLatitudeDirection.SelectedValue.ToString() == "S")
            {
                val = -val;
            }
            _latAxis.Maximum = val;
            UpdateQuery();
        }

        /*
         * Function that executes when the lower longitude bound is changed.
        */
        public void LowerLonBoundChanged(object sender, TextChangedEventArgs e)
        {
            var val = float.Parse(string.IsNullOrWhiteSpace(LowerLongitudeTextBox.Text) ? "0" : LowerLongitudeTextBox.Text);
            if (LowerLongitudeDirection != null && LowerLongitudeDirection.SelectedValue.ToString() == "W")
            {
                val = -val;
            }

            _lonAxis.Minimum = val;
            UpdateQuery();
        }

        /*
         * Function that executes when the lower longitude direction is changed.
        */
        public void LowerLonDirChanged(object sender, SelectionChangedEventArgs e)
        {
            var val = float.Parse(string.IsNullOrWhiteSpace(LowerLongitudeTextBox.Text) ? "0" : LowerLongitudeTextBox.Text);
            if (LowerLongitudeDirection.SelectedValue == null || LowerLongitudeDirection.SelectedValue.ToString() == "W")
            {
                val = -val;
            }
            _lonAxis.Minimum = val;
            UpdateQuery(); 
        }

        /*
         * Function that executes when the upper longitude bound is changed.
        */
        public void UpperLonBoundChanged(object sender, TextChangedEventArgs e)
        {
            var val = float.Parse(string.IsNullOrWhiteSpace(UpperLongitudeTextBox.Text) ? "0" : UpperLongitudeTextBox.Text);
            if (UpperLongitudeDirection != null && UpperLongitudeDirection.SelectedValue.ToString() == "W")
            {
                val = -val;
            }

            _lonAxis.Maximum = val;
            UpdateQuery();
        }

        /*
         * Function that executes when the upper longitude direction is changed.
        */
        public void UpperLonDirChanged(object sender, SelectionChangedEventArgs e)
        {
            var val = float.Parse(string.IsNullOrWhiteSpace(UpperLongitudeTextBox.Text) ? "0" : UpperLongitudeTextBox.Text);
            if (UpperLongitudeDirection.SelectedValue == null || UpperLongitudeDirection.SelectedValue.ToString() == "W")
            {
                val = -val;
            }
            _lonAxis.Maximum = val;

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

        public void GraphZoomed(object sender, AxisChangedEventArgs e)
        {
            // update selection boxes to display correct values
            // if lat actual min < 0, lower lat val change to pos, make direction S
            var lowerLatVal = _latAxis.ActualMinimum;
            if (lowerLatVal < 0)
            {
                lowerLatVal = -lowerLatVal;
                LowerLatitudeDirection.SelectedIndex = 1;
            }
            else
            {
                LowerLatitudeDirection.SelectedIndex = 0;
            }
            LowerLatitudeTextBox.Text = lowerLatVal.ToString("n1");

            var upperLatVal = _latAxis.ActualMaximum;
            if (upperLatVal < 0)
            {
                upperLatVal = -upperLatVal;
                UpperLatitudeDirection.SelectedIndex = 1;
            }
            else
            {
                UpperLatitudeDirection.SelectedIndex = 0;
            }
            UpperLatitudeTextBox.Text = upperLatVal.ToString("n1");

            var lowerLongVal = _lonAxis.ActualMinimum;
            if (lowerLongVal < 0)
            {
                lowerLongVal = -lowerLongVal;
                LowerLongitudeDirection.SelectedIndex = 1;
            }
            else
            {
                LowerLongitudeDirection.SelectedIndex = 0;
            }
            LowerLongitudeTextBox.Text = lowerLongVal.ToString("n1");

            var upperLongVal = _lonAxis.ActualMaximum;
            if (upperLongVal < 0)
            {
                upperLongVal = -upperLongVal;
                UpperLongitudeDirection.SelectedIndex = 1;
            }
            else
            {
                UpperLongitudeDirection.SelectedIndex = 0;
            }
            UpperLongitudeTextBox.Text = upperLongVal.ToString("n1");

            UpdateQuery();
        }

        /* 
         * Updates the current database query and 
         * adds the queried points to the list of points to be displayed.
        */
        private void UpdateQuery()
        {
            // clear the old paths
            PathPlot.Series.Clear();

            // open the database connection for writing
            _cycloneDatabase.DbConnection.Open();

            // make the query based on selected bounds
            var sql = "select * from cyclones " +
                      $"WHERE latitudeValue BETWEEN {_latAxis.Minimum} AND {_latAxis.Maximum} " +
                      $"AND longitudeValue BETWEEN {_lonAxis.Minimum} AND {_lonAxis.Maximum} " +
                      $"AND date BETWEEN date('{_startYearBound}-01-01') AND date('{_endYearBound}-12-31') " +
                      "ORDER BY date";

            // make sql query for the database out of the command string
            var command = new SQLiteCommand(sql, _cycloneDatabase.DbConnection);

            // read the points accpeted by the query
            var reader = command.ExecuteReader();

            DateTime prevDate = DateTime.MinValue;
            DateTime currentDate;
            string currentStormID;
            string prevStormID = null;
            CyclonePath currentPath = null;

            while (reader.Read())
            {
                // get date and storm ID
                var date = reader["date"].ToString();
                currentStormID = reader["stormID"].ToString();
                if (prevStormID == null)
                {
                    prevStormID = currentStormID;
                    currentPath = new CyclonePath(currentStormID);
                }

                // get location values, round to nearest tenth
                var lonPoint = float.Parse(reader["longitudeValue"].ToString());
                lonPoint = (float)Math.Round(lonPoint, 1, MidpointRounding.AwayFromZero);

                // get longitude direction
                var lonDir = reader["longitudeDirection"].ToString();

                // get latitude value and round it to nearest tenth
                var latPoint = float.Parse(reader["latitudeValue"].ToString());
                latPoint = (float)Math.Round(latPoint, 1, MidpointRounding.AwayFromZero);

                // get latitude direction
                var latDir = reader["latitudeDirection"].ToString();

                // add new CyclonePoint to list of points to be displayed
                var point = new CyclonePoint(date, latPoint, latDir, lonPoint, lonDir);
                currentDate = point.Date;
                if (prevDate == DateTime.MinValue)
                {
                    prevDate = currentDate;
                }

                if (prevDate.Year != currentDate.Year)
                {
                    if (currentStormID != prevStormID)
                    {
                        PathPlot.Series.Add(currentPath.path);
                        currentPath = new CyclonePath(currentStormID);
                        currentPath.AddPoint(point);
                        prevDate = currentDate;
                        prevStormID = currentStormID;
                    } else
                    {
                        if ((currentDate - prevDate).TotalDays < 365)
                            currentPath.AddPoint(point);
                        else
                        {
                            PathPlot.Series.Add(currentPath.path);
                            currentPath = new CyclonePath(currentStormID);
                            currentPath.AddPoint(point);
                            prevStormID = currentStormID;
                        }
                    }
                }
                else
                {
                    if (currentStormID == prevStormID)
                    {
                        currentPath.AddPoint(point);
                    }
                    else
                    {
                        PathPlot.Series.Add(currentPath.path);
                        currentPath = new CyclonePath(currentStormID);
                        currentPath.AddPoint(point);
                        prevStormID = currentStormID;
                    }
                }
            }
            if (currentPath != null) 
                PathPlot.Series.Add(currentPath.path);

            // we are done reading from the database. close the connection
            _cycloneDatabase.DbConnection.Close();

            // force plot to update
            PathPlot.InvalidatePlot(true);
        }

        // checks latitude and longitude value inputs
        public void CheckLowerLatitudeInput(object sender, TextCompositionEventArgs e)
        {
            var value = ((TextBox)sender).Text + e.Text;
            var handled = true;

            var lowerLatVal = IsLatitudeAllowed(value);

            if (lowerLatVal != null)
            {
                var lowerLatDir = LowerLatitudeDirection.SelectedIndex;
                var upperLatDir = UpperLatitudeDirection.SelectedIndex;
                var upperLatVal = IsLatitudeAllowed(UpperLatitudeTextBox.Text);

                if (lowerLatDir == 1)
                {
                    if (upperLatDir == 1)
                    {
                        handled = !(lowerLatVal > upperLatVal);
                    }
                    else
                    {
                        handled = false;
                    }
                }
                else
                {
                    if (upperLatDir == 0)
                    {
                        handled = !(lowerLatVal < upperLatVal);
                    }
                    else
                    {
                        handled = false;
                    }
                }
            }
            e.Handled = handled;
        }

        public void CheckUpperLatitudeInput(object sender, TextCompositionEventArgs e)
        {
            var value = ((TextBox)sender).Text + e.Text;
            var handled = true;

            var upperLatVal = IsLatitudeAllowed(value);
            if (upperLatVal != null)
            {
                var upperLatDir = UpperLatitudeDirection.SelectedIndex;
                var lowerLatDir = LowerLatitudeDirection.SelectedIndex;
                var lowerLatVal = IsLatitudeAllowed(LowerLatitudeTextBox.Text);

                if (upperLatDir == 1)
                {
                    if (lowerLatDir == 1)
                    {
                        handled = !(upperLatVal < lowerLatVal);
                    }
                    else
                    {
                        handled = false;
                    }
                }
                else
                {
                    if (lowerLatDir == 0)
                    {
                        handled = !(upperLatVal > lowerLatVal);
                    }
                    else
                    {
                        handled = false;
                    }
                }
            }
            e.Handled = handled;
        }

        public void CheckLowerLongitudeInput(object sender, TextCompositionEventArgs e)
        {
            var value = ((TextBox)sender).Text + e.Text;
            var handled = true;

            var lowerLonVal = IsLongitudeAllowed(value);
            if (lowerLonVal != null)
            {
                var lowerLonDir = LowerLongitudeDirection.SelectedIndex;
                var upperLonDir = UpperLongitudeDirection.SelectedIndex;
                var upperLonVal = IsLongitudeAllowed(UpperLongitudeTextBox.Text);

                if (lowerLonDir == 1)
                {
                    if (upperLonDir == 1)
                    {
                        handled = !(lowerLonVal > upperLonVal);
                    }
                    else
                    {
                        handled = false;
                    }
                }
                else
                {
                    if (upperLonDir == 0)
                    {
                        handled = !(lowerLonVal < upperLonVal);
                    }
                    else
                    {
                        handled = false;
                    }
                }
            }
            e.Handled = handled;
        }

        public void CheckUpperLongitudeInput(object sender, TextCompositionEventArgs e)
        {
            var value = ((TextBox)sender).Text + e.Text;
            var handled = true;

            var upperLonVal = IsLongitudeAllowed(value);
            if (upperLonVal != null)
            {
                var upperLonDir = UpperLongitudeDirection.SelectedIndex;
                var lowerLonDir = LowerLongitudeDirection.SelectedIndex;
                var lowerLonVal = IsLongitudeAllowed(LowerLongitudeTextBox.Text);

                if (upperLonDir == 1)
                {
                    if (lowerLonDir == 1)
                    {
                        handled = !(upperLonVal < lowerLonVal);
                    }
                    else
                    {
                        handled = false;
                    }
                }
                else
                {
                    if (lowerLonDir == 0)
                    {
                        handled = !(upperLonVal > lowerLonVal);
                    }
                    else
                    {
                        handled = false;
                    }
                }
            }
            e.Handled = handled;
        }

        private decimal? IsLatitudeAllowed(string str)
        {
            decimal dummy;
            if (!(decimal.TryParse(str, out dummy))) return null;

            var latVal = dummy;
            if (latVal >= 0 && latVal <= 90)
            {
                return latVal;
            }
            return null;
        }

        private decimal? IsLongitudeAllowed(string str)
        {
            decimal dummy;
            if (!(decimal.TryParse(str, out dummy))) return null;

            var lonVal = dummy;
            if (lonVal >= 0 && lonVal <= 180)
            {
                return lonVal;
            }
            return null;
        }
    }
}