using System.Data.SQLite;
using System.IO;

namespace VisualCycloneGUI.Cyclone
{
    public class CycloneDatabase
    {
        public static SQLiteConnection DbConnection;

        public static void CreateDatabaseConnection(string filename)
        {
            var databaseBuilt = File.Exists(filename);

            if (!databaseBuilt)
                SQLiteConnection.CreateFile(filename);

            DbConnection = new SQLiteConnection($"Data Source={filename};Version=3;");

            if (databaseBuilt) return;
            DbConnection.Open();

            const string sql = "create table cyclones (" +
                               "date date, " +
                               "latitudeValue decimal(3, 1), " +
                               "latitudeDirection char(1), " +
                               "longitudeValue decimal(3, 1)," +
                               "longitudeDirection char(1))";

            var command = new SQLiteCommand(sql, DbConnection);
            command.ExecuteNonQuery();

            DbConnection.Close();
        }
    }
}
