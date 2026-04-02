using System.Collections.Generic;
using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Smuggler.Common.DataBase;

/// <summary>
/// SQLite + Dapper 기반 DB 에이전트 구현체.
/// </summary>
public class DapperAgent : IDBAgent
{
    private string? _connectionString;
    private readonly ILogger<DapperAgent>? _logger;

    /// <param name="logger">예외 로깅에 사용할 ILogger. null이면 로깅하지 않는다.</param>
    public DapperAgent(ILogger<DapperAgent>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">dbFilePath가 null이거나 비어 있을 때.</exception>
    public bool SetConnection(string dbFilePath)
    {
        if (string.IsNullOrWhiteSpace(dbFilePath))
            throw new ArgumentException("DB 파일 경로가 비어 있습니다.", nameof(dbFilePath));

        if (!File.Exists(dbFilePath))
        {
            _logger?.LogWarning("DB 파일을 찾을 수 없습니다: {Path}", dbFilePath);
            return false;
        }

        string connectionString = BuildConnectionString(dbFilePath);

        try
        {
            // C0201: SQLite 호환 연결 확인 쿼리 (SELECT 1)
            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();
            connection.ExecuteScalar<int>("SELECT 1");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"DB 연결에 실패했습니다. 경로: {dbFilePath} / 원인: {ex.Message}", ex);
        }

        _connectionString = connectionString;
        _logger?.LogInformation("DB 연결 설정 완료: {Path}", dbFilePath);
        return true;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">SetConnection이 호출되지 않았을 때.</exception>
    public IEnumerable<T> Query<T>(string sql, object? param = null)
    {
        using SqliteConnection connection = CreateConnection();
        try
        {
            return connection.Query<T>(sql, param);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Query 실행 실패. SQL: {Sql}", sql);
            throw;
        }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">SetConnection이 호출되지 않았을 때.</exception>
    public T? QuerySingle<T>(string sql, object? param = null)
    {
        using SqliteConnection connection = CreateConnection();
        try
        {
            return connection.QueryFirstOrDefault<T>(sql, param);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "QuerySingle 실행 실패. SQL: {Sql}", sql);
            throw;
        }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">SetConnection이 호출되지 않았을 때.</exception>
    public int Execute(string sql, object? param = null)
    {
        using SqliteConnection connection = CreateConnection();
        try
        {
            return connection.Execute(sql, param);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Execute 실행 실패. SQL: {Sql}", sql);
            throw;
        }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">SetConnection이 호출되지 않았을 때.</exception>
    public int ExecuteInTransaction(Func<IDbConnection, IDbTransaction, int> work)
    {
        using SqliteConnection connection = CreateConnection();
        connection.Open();
        using IDbTransaction transaction = connection.BeginTransaction();
        try
        {
            int result = work(connection, transaction);
            transaction.Commit();
            return result;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger?.LogError(ex, "트랜잭션 실행 중 오류 발생. 롤백됨.");
            throw;
        }
    }

    // -------------------------------------------------------------------------

    /// <summary>DB 파일 경로로 SQLite 연결 문자열을 생성한다.</summary>
    private static string BuildConnectionString(string dbFilePath)
        => $"Data Source={dbFilePath}";

    /// <summary>
    /// 현재 연결 문자열로 SqliteConnection을 생성한다.
    /// SetConnection이 호출되지 않았으면 InvalidOperationException을 던진다.
    /// </summary>
    private SqliteConnection CreateConnection()
    {
        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("DB 연결이 설정되지 않았습니다. SetConnection()을 먼저 호출하세요.");

        return new SqliteConnection(_connectionString);
    }
}
