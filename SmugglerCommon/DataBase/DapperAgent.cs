using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Smuggler.Common.DataBase;

public class DapperAgent : DBAgent
{
    public DapperAgent(string? databasePath = null, ILogger<DapperAgent>? logger = null)
        : base(databasePath, logger)
    {
    }

    public override IReadOnlyList<T> Query<T>(string sql, object? param = null, IDbTransaction? transaction = null)
    {
        ValidateSql(sql);

        if (transaction is not null)
        {
            return transaction.Connection!.Query<T>(sql, param, transaction).AsList();
        }

        using var connection = CreateOpenedConnection();
        return connection.Query<T>(sql, param).AsList();
    }

    public override T QuerySingle<T>(string sql, object? param = null, IDbTransaction? transaction = null)
    {
        ValidateSql(sql);

        if (transaction is not null)
        {
            return transaction.Connection!.QuerySingle<T>(sql, param, transaction);
        }

        using var connection = CreateOpenedConnection();
        return connection.QuerySingle<T>(sql, param);
    }

    public override int Execute(string sql, object? param = null, IDbTransaction? transaction = null)
    {
        ValidateSql(sql);

        if (transaction is not null)
        {
            return transaction.Connection!.Execute(sql, param, transaction);
        }

        using var connection = CreateOpenedConnection();
        return connection.Execute(sql, param);
    }

    public override T ExecuteInTransaction<T>(Func<IDbConnection, IDbTransaction, T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var connection = CreateOpenedConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            T result = action(connection, transaction);
            transaction.Commit();
            return result;
        }
        catch (Exception exception)
        {
            try
            {
                transaction.Rollback();
            }
            catch (Exception rollbackException)
            {
                Logger?.LogError(rollbackException, "SQLite transaction rollback failed for {DatabasePath}", DatabasePath);
            }

            Logger?.LogError(exception, "SQLite transaction failed for {DatabasePath}", DatabasePath);
            throw new InvalidOperationException($"SQLite transaction failed for '{DatabasePath}'.", exception);
        }
    }

    private static void ValidateSql(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentException("SQL is required.", nameof(sql));
        }
    }
}
