using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Smuggler.Common.DataBase;

public abstract class DBAgent : IDBAgent
{
    private readonly ILogger? _logger;

    protected DBAgent(string? databasePath = null, ILogger? logger = null)
    {
        _logger = logger;

        if (!string.IsNullOrWhiteSpace(databasePath))
        {
            SetConnection(databasePath);
        }
    }

    public string DatabasePath { get; private set; } = string.Empty;

    public string ConnectionString { get; private set; } = string.Empty;

    protected ILogger? Logger => _logger;

    public bool SetConnection(string databasePath)
    {
        DatabasePath = ValidateDatabasePath(databasePath);
        ConnectionString = BuildConnectionString(DatabasePath);

        using SqliteConnection connection = CreateOpenedConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT 1;";
        _ = command.ExecuteScalar();

        return true;
    }

    public bool CanConnect()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            return false;
        }

        try
        {
            using SqliteConnection connection = CreateOpenedConnection();
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT 1;";
            object? result = command.ExecuteScalar();
            return Convert.ToInt32(result) == 1;
        }
        catch (Exception exception)
        {
            Logger?.LogError(exception, "SQLite connection check failed for {DatabasePath}", DatabasePath);
            return false;
        }
    }

    protected SqliteConnection CreateOpenedConnection()
    {
        EnsureConnectionConfigured();

        try
        {
            SqliteConnection connection = new(ConnectionString);
            connection.Open();
            return connection;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Failed to open SQLite connection for '{DatabasePath}'.", exception);
        }
    }

    protected void EnsureConnectionConfigured()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            throw new InvalidOperationException("Database connection has not been configured. Call SetConnection first.");
        }
    }

    protected static string ValidateDatabasePath(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("Database path is required.", nameof(databasePath));
        }

        string fullPath = Path.GetFullPath(databasePath);
        string? directory = Path.GetDirectoryName(fullPath);

        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Database directory does not exist: '{directory ?? fullPath}'.");
        }

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"SQLite database file does not exist: '{fullPath}'.", fullPath);
        }

        return fullPath;
    }

    protected static string BuildConnectionString(string databasePath)
    {
        SqliteConnectionStringBuilder builder = new()
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWrite,
            Cache = SqliteCacheMode.Shared,
        };

        return builder.ToString();
    }

    public abstract IReadOnlyList<T> Query<T>(string sql, object? param = null, System.Data.IDbTransaction? transaction = null);

    public abstract T QuerySingle<T>(string sql, object? param = null, System.Data.IDbTransaction? transaction = null);

    public abstract int Execute(string sql, object? param = null, System.Data.IDbTransaction? transaction = null);

    public abstract T ExecuteInTransaction<T>(Func<System.Data.IDbConnection, System.Data.IDbTransaction, T> action);
}
