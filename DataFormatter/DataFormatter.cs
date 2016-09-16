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
    }
}
