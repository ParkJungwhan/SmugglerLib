using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Smuggler.Common;
using Smuggler.Common.DataBase;

namespace TestConsole;

public static class DatabaseExample
{
    public static void Run()
    {
        string dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDirectory);

        string databasePath = Path.Combine(dataDirectory, "sample.db");
        EnsureDatabase(databasePath);

        using ILoggerFactory factory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddSmugLogger(config => config.MinimumLogLevel = LogLevel.Information);
        });

        ILogger<DapperAgent> logger = factory.CreateLogger<DapperAgent>();
        DapperAgent agent = new(databasePath, logger);

        agent.Execute("INSERT INTO Sample(Name) VALUES (@Name);", new { Name = "delta" });
        long count = agent.QuerySingle<long>("SELECT COUNT(*) FROM Sample;");
        IReadOnlyList<SampleRow> rows = agent.Query<SampleRow>("SELECT Id, Name FROM Sample ORDER BY Id DESC LIMIT 3;");

        Console.WriteLine($"Database connected: {agent.CanConnect()}");
        Console.WriteLine($"Row count: {count}");

        foreach (SampleRow row in rows)
        {
            Console.WriteLine($"  {row.Id}: {row.Name}");
        }
    }

    private static void EnsureDatabase(string databasePath)
    {
        if (File.Exists(databasePath))
        {
            return;
        }

        SqliteConnectionStringBuilder builder = new()
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
        };

        using SqliteConnection connection = new(builder.ToString());
        connection.Open();

        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE Sample (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL); INSERT INTO Sample(Name) VALUES ('alpha'), ('beta');";
        command.ExecuteNonQuery();
    }

    private sealed class SampleRow
    {
        public long Id { get; init; }

        public string Name { get; init; } = string.Empty;
    }
}
