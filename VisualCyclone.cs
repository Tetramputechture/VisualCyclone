using System;
using System.IO;

namespace VisualCyclone
{
    internal class VisualCyclone
    {
        private static void Main(string[] args)
        {
            const string rawDataFolderName = "RawCycloneData";

            var formattedDataFolderPath = Path.Combine(Environment.
                GetFolderPath(
                    Environment.SpecialFolder.CommonApplicationData
                ),
                "CycloneProjectData",
                "FormattedCycloneData");

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
                DataFormatter.DataFormatter.FormatDirectory(rawDataFolderName, formattedDataFolderPath);
            }
            Console.Read();
        }
    }
}