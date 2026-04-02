using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;
using Microsoft.Data.Sqlite;
using Smuggler.Common.DataBase;

namespace SmugglerTDDs;

/// <summary>
/// C0206: DapperAgent SQLite 기반 연결/조회 테스트
/// </summary>
public class DBAgentTests : IDisposable
{
    // 테스트용 임시 SQLite DB 파일 경로
    private readonly string _dbPath;
    private readonly DapperAgent _agent;

    public DBAgentTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"smug_test_{Guid.NewGuid():N}.db");
        _agent = new DapperAgent();
        InitializeTestDb(_dbPath);
    }

    public void Dispose()
    {
        // SQLite 연결 풀이 파일을 점유하지 않도록 풀을 비운 뒤 삭제
        SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }

    // -------------------------------------------------------------------------
    // C0202: SetConnection - 파일 경로 검증
    // -------------------------------------------------------------------------

    [Fact]
    public void SetConnection_NullPath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _agent.SetConnection(null!));
    }

    [Fact]
    public void SetConnection_EmptyPath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _agent.SetConnection(""));
    }

    [Fact]
    public void SetConnection_WhitespacePath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _agent.SetConnection("   "));
    }

    [Fact]
    public void SetConnection_NonExistentFile_ReturnsFalse()
    {
        bool result = _agent.SetConnection(@"C:\nonexistent\path\file.db");

        Assert.False(result);
    }

    [Fact]
    public void SetConnection_ValidFile_ReturnsTrue()
    {
        bool result = _agent.SetConnection(_dbPath);

        Assert.True(result);
    }

    // -------------------------------------------------------------------------
    // C0201: SetConnection - SQLite 호환 연결 확인 쿼리 검증
    // -------------------------------------------------------------------------

    [Fact]
    public void SetConnection_ValidFile_EstablishesWorkingConnection()
    {
        // SetConnection 후 Query가 정상 동작하면 연결이 제대로 설정된 것
        _agent.SetConnection(_dbPath);
        IEnumerable<int> result = _agent.Query<int>("SELECT 1");

        Assert.Single(result);
        Assert.Equal(1, result.First());
    }

    // -------------------------------------------------------------------------
    // C0203: Query / QuerySingle / Execute 래퍼 검증
    // -------------------------------------------------------------------------

    [Fact]
    public void Query_ReturnsAllRows()
    {
        _agent.SetConnection(_dbPath);

        IEnumerable<TestItem> items = _agent.Query<TestItem>("SELECT Id, Name FROM Items");

        Assert.Equal(3, items.Count());
    }

    [Fact]
    public void Query_WithParam_ReturnsFilteredRows()
    {
        _agent.SetConnection(_dbPath);

        IEnumerable<TestItem> items = _agent.Query<TestItem>(
            "SELECT Id, Name FROM Items WHERE Id = @Id",
            new { Id = 2 });

        Assert.Single(items);
        Assert.Equal("Item2", items.First().Name);
    }

    [Fact]
    public void QuerySingle_ExistingRow_ReturnsValue()
    {
        _agent.SetConnection(_dbPath);

        TestItem? item = _agent.QuerySingle<TestItem>(
            "SELECT Id, Name FROM Items WHERE Id = @Id",
            new { Id = 1 });

        Assert.NotNull(item);
        Assert.Equal("Item1", item.Name);
    }

    [Fact]
    public void QuerySingle_NoMatchingRow_ReturnsDefault()
    {
        _agent.SetConnection(_dbPath);

        TestItem? item = _agent.QuerySingle<TestItem>(
            "SELECT Id, Name FROM Items WHERE Id = @Id",
            new { Id = 999 });

        Assert.Null(item);
    }

    [Fact]
    public void Execute_InsertsRow_AffectedRowCountIsOne()
    {
        _agent.SetConnection(_dbPath);

        int affected = _agent.Execute(
            "INSERT INTO Items (Name) VALUES (@Name)",
            new { Name = "Item4" });

        Assert.Equal(1, affected);

        IEnumerable<TestItem> items = _agent.Query<TestItem>("SELECT Id, Name FROM Items");
        Assert.Equal(4, items.Count());
    }

    // -------------------------------------------------------------------------
    // C0204: ExecuteInTransaction 검증
    // -------------------------------------------------------------------------

    [Fact]
    public void ExecuteInTransaction_Success_CommitsChanges()
    {
        _agent.SetConnection(_dbPath);

        _agent.ExecuteInTransaction((conn, tx) =>
        {
            return conn.Execute(
                "INSERT INTO Items (Name) VALUES (@Name)",
                new { Name = "TxItem" },
                tx);
        });

        TestItem? inserted = _agent.QuerySingle<TestItem>(
            "SELECT Id, Name FROM Items WHERE Name = 'TxItem'");

        Assert.NotNull(inserted);
    }

    [Fact]
    public void ExecuteInTransaction_ExceptionInWork_RollsBack()
    {
        _agent.SetConnection(_dbPath);
        int countBefore = _agent.Query<TestItem>("SELECT Id, Name FROM Items").Count();

        Assert.Throws<InvalidOperationException>(() =>
        {
            _agent.ExecuteInTransaction((conn, tx) =>
            {
                conn.Execute(
                    "INSERT INTO Items (Name) VALUES (@Name)",
                    new { Name = "WillBeRolledBack" },
                    tx);
                throw new InvalidOperationException("의도적 예외");
            });
        });

        int countAfter = _agent.Query<TestItem>("SELECT Id, Name FROM Items").Count();
        Assert.Equal(countBefore, countAfter);
    }

    [Fact]
    public void Execute_WithoutSetConnection_ThrowsInvalidOperationException()
    {
        var agent = new DapperAgent();

        Assert.Throws<InvalidOperationException>(() =>
            agent.Execute("SELECT 1"));
    }

    // -------------------------------------------------------------------------
    // 헬퍼
    // -------------------------------------------------------------------------

    private static void InitializeTestDb(string dbPath)
    {
        using SqliteConnection conn = new SqliteConnection($"Data Source={dbPath}");
        conn.Open();
        using SqliteCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE Items (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL);
            INSERT INTO Items (Name) VALUES ('Item1');
            INSERT INTO Items (Name) VALUES ('Item2');
            INSERT INTO Items (Name) VALUES ('Item3');";
        cmd.ExecuteNonQuery();
    }

    private class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
