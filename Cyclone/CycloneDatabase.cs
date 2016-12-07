using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;

namespace VisualCycloneGUI.Cyclone
{
    /*
     * A Cyclone Database that stores cyclone information in an SQLite database. 
    */

    public class CycloneDatabase
    {
        /*
         * The SQLiteConnection associated with this database. 
        */
        public SQLiteConnection DbConnection { get; }

        /*
         * The fileName associated with this database.
        */
        public string DbFileName { get; }

        /*
         * If this database has been created before.
        */
        public bool IsExistingDb { get; }

        /*
         * Constructs a new CycloneDatabase with the specified fileName,
         * or establishes a connection with an existing cyclone database.
        */

        public CycloneDatabase(string fileName)
        {
            DbFileName = fileName;

            // check if file already exists
            IsExistingDb = File.Exists(DbFileName);

            // create file if it doesn't
            if (!IsExistingDb)
                SQLiteConnection.CreateFile(DbFileName);

            // establish database connection with file
            DbConnection = new SQLiteConnection($"Data Source={DbFileName};Version=3;datetimeformat=CurrentCulture");

            // if the database hasn't been built yet, build it. otherwise the connection is made and we are done. 
            if (IsExistingDb) return;

            // open connection for writing
            DbConnection.Open();

            // create table
            // format: date, storm ID, latitude (in degrees north), longitude (in degrees east)
            const string sql = "create table cyclones (" +
                               "date date, " +
                               "stormID integer, " +
                               "latitudeValue decimal(3, 1), " +
                               "latitudeDirection char(1), " +
                               "longitudeValue decimal(3, 1)," +
                               "longitudeDirection char(1))";

            var command = new SQLiteCommand(sql, DbConnection);
            command.ExecuteNonQuery();

            // close connection
            DbConnection.Close();
        }

        /*
         * Parses a file and inserts the relevant data into this database. 
        */

        private void InsertDataFromFile(string fileName)
        {
            // iterate through lines of file
            var lines = File.ReadLines(fileName);
            foreach (var lineData in lines.Select(line => line.Split(',')))
            {
                // date is 3rd value, stormID is 2nd, lat/lon are 7th and 8th
                // make date in format yyyy-mm-dd hh:mm
                var date = lineData[2].Replace(" ", "");
                date = date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2) + " " +
                       date.Substring(8, 2) + ":00";

                var stormID = int.Parse(lineData[1]);

                // parse lat/lon with a regex. splits it into two parts: number and direction
                var regex = new Regex(@"(\d+)([a-zA-Z]+)");

                var latitude = regex.Match(lineData[6].Replace(" ", ""));

                // but wait! values are stored in tenths of degrees. e.g. 78 = 7.8, 152 = 15.2
                // divide by 10 to solve this
                // also, latitude values are stored in degrees north, and longitude values in degrees east
                // to solve this, if latitude direction is "S" or longitude direction is "W", make value negative
                var latVal = float.Parse(latitude.Groups[1].Value)/10f;
                var latDir = Convert.ToChar(latitude.Groups[2].Value);
                if (latDir == 'S')
                {
                    latVal = -latVal;
                }

                var longitude = regex.Match(lineData[7].Replace(" ", ""));

                var lonVal = float.Parse(longitude.Groups[1].Value)/10f;
                var lonDir = Convert.ToChar(longitude.Groups[2].Value);
                if (lonDir == 'W')
                {
                    lonVal = -lonVal;
                }

                var sql = "insert into cyclones " +
                          "values ('" + date + "', + " + stormID + ", " + latVal + ", '" + latDir + "', " + lonVal + ", '" + lonDir +
                          "')";

                var command = new SQLiteCommand(sql, DbConnection);
                command.ExecuteNonQuery();
            }
        }

        /*
         * Reads through each file in a directory and inserts the relevant data into this database. 
        */

        public void InsertDataFromDirectory(string dirName)
        {
            using (var tran = new TransactionScope())
            {
                DbConnection.Open();

                string[] years;

                // get each directory (year) within raw data
                try
                {
                    years = Directory.GetDirectories(dirName);
                }

                    // if any sort of failure, delete the directory so program must database again
                catch (Exception ex)
                {
                    DatabaseInsertionFailure(ex);
                    return;
                }

                foreach (var file in years.Select(Directory.GetFiles).SelectMany(files => files))
                {
                    // create formatted file with year directory
                    try
                    {
                        InsertDataFromFile(file);
                    }
                    catch (Exception ex)
                    {
                        DatabaseInsertionFailure(ex);
                        return;
                    }
                }

                DbConnection.Close();
                tran.Complete();
            }
        }

        private void DatabaseInsertionFailure(Exception ex)
        {
            Console.WriteLine("Database creation failed! Error message: " + ex.Message);
            // close database connection if we encounter an exception while the connection is open
            DbConnection.Close();
        }
    }
}
