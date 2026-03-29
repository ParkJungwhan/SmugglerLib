using Microsoft.Data.Sqlite;

namespace SmugCommon.DataBase
{
    public class DapperAgent : IDBAgent
    {
        /*
        using var connection = new SqliteConnection("Data Source=Blogs.db");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Url FROM Blogs";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var url = reader.GetString(0);
        }
        */

        public bool SetConnection(string strDBFile)
        {
            if (string.IsNullOrEmpty(strDBFile))
            {
                throw new ArgumentNullException();
            }

            if (false == File.Exists(strDBFile)) return false;

            //"Data Source=Blogs.db"
            string strConnect = $"Data Source={strDBFile}";

            using var connection = new SqliteConnection(strConnect);
            connection.Open();

            using var command = connection.CreateCommand();
            //command.CommandText = "SELECT Url FROM Blogs";
            command.CommandText = "SELECT now()";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var url = reader.GetString(0);
            }

            return true;
        }
    }
}