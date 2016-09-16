using System;
using System.IO;
using VisualCyclone.Cyclone;

namespace VisualCyclone
{
    internal class VisualCyclone
    {
        // name of the folder containing the raw cyclone data
        private const string RawDataFolderName = "RawCycloneData";

        // path of the formatted cyclone data on the users computer
        private static readonly string FormattedDataFolderPath = Path.Combine(Environment.
                GetFolderPath(
                    Environment.SpecialFolder.CommonApplicationData
                ),
                "CycloneProjectData",
                "FormattedCycloneData");

        private static void Main(string[] args)
        {
            //// on first startup, format the data into a usable form
            //if (!Directory.Exists(FormattedDataFolderPath))
            //{
            //    FormatRawData();
            //}
            
            // for dev purposes, ask the user if they want to format the data on startup
            Console.Write("Would you like to format the raw data into a usable format? Y/N\n");
            var answer = Console.ReadLine();
            if (answer == "Y")
            {
                FormatRawData();
            }
            Console.Read();
        }

        // formats all raw cyclone data by iterating through each files in each year
        private static void FormatRawData()
        {
            Console.WriteLine("Formatting data for use...");

            // create formatted cyclone data directory
            Directory.CreateDirectory(FormattedDataFolderPath);

            string[] years;

            // get each directory (year) within raw data
            try
            {
                years = Directory.GetDirectories(RawDataFolderName);
            }

            // if any sort of failure, delete the directory so program must rebuild formatted data again
            catch (Exception ex)
            {
                FormatFailure(ex);
                return;
            }

            foreach (var year in years)
            {
                var formattedYearPath = Path.Combine(FormattedDataFolderPath, Path.GetFileName(year));

                // create year directory within formatted data directory
                Directory.CreateDirectory(formattedYearPath);

                var files = Directory.GetFiles(year);

                foreach (var file in files)
                {
                    // create formatted file with year directory
                    try
                    {
                        DataFormatter.DataFormatter.FormatFile(file, Path.GetFileName(file),
                            formattedYearPath);
                    }
                    catch (Exception ex)
                    {
                        FormatFailure(ex);
                        return;
                    }
                }
            }

            Console.WriteLine("Data formatted. Formatted data located on your computer in " + FormattedDataFolderPath);
        }

        private static void FormatFailure(Exception ex)
        {
            Console.WriteLine("Data format failed! Error message: " + ex.Message);
            Directory.Delete(FormattedDataFolderPath, true);
        }
    }
}