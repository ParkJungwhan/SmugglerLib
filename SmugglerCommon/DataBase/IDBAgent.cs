using System.Data;

namespace Smuggler.Common.DataBase;

public interface IDBAgent
{
    string DatabasePath { get; }

    string ConnectionString { get; }

    bool SetConnection(string databasePath);

    bool CanConnect();

    IReadOnlyList<T> Query<T>(string sql, object? param = null, IDbTransaction? transaction = null);

    T QuerySingle<T>(string sql, object? param = null, IDbTransaction? transaction = null);

    int Execute(string sql, object? param = null, IDbTransaction? transaction = null);

    T ExecuteInTransaction<T>(Func<IDbConnection, IDbTransaction, T> action);
}
