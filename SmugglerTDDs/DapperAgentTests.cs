using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Smuggler.Common.DataBase;

namespace SmugglerTDDs;

public sealed class DapperAgentTests : IDisposable
{
    private readonly string _databasePath;

    public DapperAgentTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"smuggler-db-{Guid.NewGuid():N}.sqlite");
        CreateSampleDatabase(_databasePath);
    }

    [Fact]
    public void SetConnection_UsesSqliteCompatibleHealthQuery()
    {
        DapperAgent agent = new();

        bool connected = agent.SetConnection(_databasePath);

        Assert.True(connected);
        Assert.True(agent.CanConnect());
        Assert.Equal(Path.GetFullPath(_databasePath), agent.DatabasePath);
        Assert.Contains("Data Source=", agent.ConnectionString, StringComparison.Ordinal);
    }

    [Fact]
    public void SetConnection_Throws_WhenDatabaseFileIsMissing()
    {
        DapperAgent agent = new();
        string missingPath = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.sqlite");

        FileNotFoundException exception = Assert.Throws<FileNotFoundException>(() => agent.SetConnection(missingPath));

        Assert.Contains("SQLite database file does not exist", exception.Message);
    }

    [Fact]
    public void QueryQuerySingleAndExecute_WorkAgainstSqliteFile()
    {
        DapperAgent agent = new(_databasePath);

        IReadOnlyList<SampleRow> rows = agent.Query<SampleRow>("SELECT Id, Name FROM Sample ORDER BY Id;");
        int affected = agent.Execute("INSERT INTO Sample(Name) VALUES (@Name);", new { Name = "gamma" });
        long count = agent.QuerySingle<long>("SELECT COUNT(*) FROM Sample;");

        Assert.Equal(2, rows.Count);
        Assert.Equal("alpha", rows[0].Name);
        Assert.Equal(1, affected);
        Assert.Equal(3L, count);
    }

    [Fact]
    public void ExecuteInTransaction_RollsBackAndLogs_WhenAnErrorOccurs()
    {
        ListLogger<DapperAgent> logger = new();
        DapperAgent agent = new(_databasePath, logger);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            agent.ExecuteInTransaction<int>((connection, transaction) =>
            {
                connection.Execute("INSERT INTO Sample(Name) VALUES (@Name);", new { Name = "rolled-back" }, transaction);
                throw new InvalidOperationException("boom");
            }));

        long count = agent.QuerySingle<long>("SELECT COUNT(*) FROM Sample WHERE Name = @Name;", new { Name = "rolled-back" });

        Assert.Equal(0L, count);
        Assert.Contains(logger.Messages, message => message.Contains("SQLite transaction failed", StringComparison.Ordinal));
        Assert.Contains("boom", exception.ToString(), StringComparison.Ordinal);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();

        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }

    private static void CreateSampleDatabase(string databasePath)
    {
        SqliteConnectionStringBuilder builder = new()
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
        };

        using SqliteConnection connection = new(builder.ToString());
        connection.Open();
        connection.Execute("CREATE TABLE Sample (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL);");
        connection.Execute("INSERT INTO Sample(Name) VALUES ('alpha'), ('beta');");
    }

    private sealed class ListLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }

    private sealed class SampleRow
    {
        public long Id { get; init; }

        public string Name { get; init; } = string.Empty;
    }
}
