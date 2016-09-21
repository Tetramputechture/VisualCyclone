using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using VisualCycloneGUI.Cyclone;

namespace VisualCycloneGUI.DataFormatter
{

    // formats data into a format that only includes tropical cyclones and their locations. 
    public class DataFormatter
    {
        // the version of this data format. for now, it only formats data into date, location, and time.
        // todo: have program write / enforce version
        private readonly string Version = "0.1";

        // creates a cyclone database. includes all cyclone dates and locations
        public static void CreateDatabase(bool recreateFile = false)
        {
            var databaseFolderPath = Path.Combine(Environment.
                GetFolderPath(
                    Environment.SpecialFolder.CommonApplicationData
                ),
                "CycloneProjectData",
                "CycloneDatabase");

            CycloneDatabase.CreateDatabaseConnection("cyclonestest.sqlite");

            if (!recreateFile && File.Exists("cyclonestest.sqlite")) return;

            Console.Write("Building database...\n");

            // create database from raw data
            using (var tran = new TransactionScope())
            {
                CycloneDatabase.DbConnection.Open();
                FeedDirectoryToDatabase("RawCycloneData", CycloneDatabase.DbConnection);

                CycloneDatabase.DbConnection.Close();
                tran.Complete();
            }
        }


        // takes a file, parses the data, and feeds the data to a database
        public static void FeedFileToDatabase(string fileName, SQLiteConnection db)
        {
            // iterate through lines of file
            var lines = File.ReadLines(fileName);
            foreach (var lineData in lines.Select(line => line.Split(',')))
            {
                // date is 3rd value, lat/lon are 7th and 8th
                var date = lineData[2].Replace(" ", "");

                // parse lat/lon with a regex. splits it into two parts: number and direction
                var regex = new Regex(@"(\d+)([a-zA-Z]+)");
                var latitude = regex.Match(lineData[6].Replace(" ", ""));

                // but wait! values are stored in tenths of degrees. e.g. 78 = 7.8, 152 = 15.2
                // divide by 10 to solve this
                var latVal = float.Parse(latitude.Groups[1].Value) / 10f;
                var latDir = Convert.ToChar(latitude.Groups[2].Value);

                var longitude = regex.Match(lineData[7].Replace(" ", ""));

                var lonVal = float.Parse(longitude.Groups[1].Value) / 10f;
                var lonDir = Convert.ToChar(longitude.Groups[2].Value);

                var sql = "insert into cyclones " +
                             "values ('" + date + "', + " + latVal + ", '" + latDir + "', " + lonVal + ", '" + lonDir + "')";

                var command = new SQLiteCommand(sql, db);
                command.ExecuteNonQuery();
            }
        }

        // takes a directory and parses each file in it and adds the data in each file to a specified database
        public static void FeedDirectoryToDatabase(string dirName, SQLiteConnection db)
        {
            string[] years;

            // get each directory (year) within raw data
            try
            {
                years = Directory.GetDirectories(dirName);
            }

            // if any sort of failure, delete the directory so program must database again
            catch (Exception ex)
            {
                FormatFailure(ex);
                return;
            }

            foreach (var year in years)
            {
                // create year directory within formatted data directory

                var files = Directory.GetFiles(year);
                foreach (var file in files)
                {
                    // create formatted file with year directory
                    try
                    {
                        FeedFileToDatabase(file, db);
                    }
                    catch (Exception ex)
                    {
                        FormatFailure(ex);
                        return;
                    }
                }
            }
        }

        private static void FormatFailure(Exception ex)
        {
            Console.WriteLine("Database creation failed! Error message: " + ex.Message);
        }
    }
}
