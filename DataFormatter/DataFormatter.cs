using System;
using System.IO;
using System.Linq;
using System.Text;

namespace VisualCyclone.DataFormatter
{

    // formats data into a format that only includes tropical cyclones and their locations. 
    public class DataFormatter
    {
        // the version of this data format. for now, it only formats data into date, location, and time.
        // todo: have program write / enforce version
        private readonly string Version = "0.1";

        // takes a file and parses it line by line, returning a new file that includes only locations and dates.
        public static void FormatFile(string fileName, string outputFileName, string outputFileLocation)
        {
            var filePath = outputFileLocation + "\\" + outputFileName;

            // create stringbuilder for text to be added to file
            var fileData = new StringBuilder();

            // iterate through lines of file
            var lines = File.ReadLines(fileName);
            foreach (var lineData in lines.Select(line => line.Split(',')))
            {
                // data is 3rd value, lat/lon are 7th and 8th
                fileData.AppendLine((lineData[2] + ", " + lineData[6] + ", " + lineData[7]).Replace(" ", ""));
            }

            // write dates and locations to formatted file
            File.WriteAllText(filePath, fileData.ToString());
        }

        // takes a directory and formats each file in the directory
        public static void FormatDirectory(string inputDirName, string outputDirName)
        {
            Console.WriteLine("Formatting data in directory " + inputDirName + " for use...");

            Directory.CreateDirectory(outputDirName);

            string[] years;

            // get each directory (year) within raw data
            try
            {
                years = Directory.GetDirectories(inputDirName);
            }

            // if any sort of failure, delete the directory so program must rebuild formatted data again
            catch (Exception ex)
            {
                FormatFailure(ex, outputDirName);
                return;
            }

            foreach (var year in years)
            {
                var formattedYearPath = Path.Combine(outputDirName, Path.GetFileName(year));

                // create year directory within formatted data directory
                Directory.CreateDirectory(formattedYearPath);

                var files = Directory.GetFiles(year);

                foreach (var file in files)
                {
                    // create formatted file with year directory
                    try
                    {
                        FormatFile(file, Path.GetFileName(file),
                            formattedYearPath);
                    }
                    catch (Exception ex)
                    {
                        FormatFailure(ex, outputDirName);
                        return;
                    }
                }
            }

            Console.WriteLine("Data formatted. Formatted data located on your computer in " + outputDirName);
        }

        private static void FormatFailure(Exception ex, string dirToDelete)
        {
            Console.WriteLine("Data format failed! Error message: " + ex.Message);
            Directory.Delete(dirToDelete, true);
        }
    }
}
