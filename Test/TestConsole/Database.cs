using System;
using System.Collections.Generic;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;
using Smuggler.Common.DataBase;

namespace TestConsole;

/// <summary>
/// C0207: DapperAgent SQLite 사용 예제
/// </summary>
internal static class Database
{
    private record Product(int Id, string Name, int Price);

    internal static void Run()
    {
        Console.WriteLine("=== Database 예제 시작 ===");
        Console.WriteLine();

        string dbPath = Path.Combine(Path.GetTempPath(), "smug_example.db");

        try
        {
            SetupExampleDb(dbPath);

            var agent = new DapperAgent();
            if (!agent.SetConnection(dbPath))
            {
                Console.WriteLine($"DB 파일을 찾을 수 없습니다: {dbPath}");
                return;
            }

            // 1. Query - 전체 조회
            Console.WriteLine("--- Query: 전체 상품 조회 ---");
            IEnumerable<Product> all = agent.Query<Product>("SELECT Id, Name, Price FROM Products");
            foreach (Product p in all)
                Console.WriteLine($"  [{p.Id}] {p.Name} - {p.Price:N0}원");

            Console.WriteLine();

            // 2. QuerySingle - 단건 조회
            Console.WriteLine("--- QuerySingle: Id=2 조회 ---");
            Product? found = agent.QuerySingle<Product>(
                "SELECT Id, Name, Price FROM Products WHERE Id = @Id",
                new { Id = 2 });
            Console.WriteLine(found is not null ? $"  {found.Name} - {found.Price:N0}원" : "  없음");

            Console.WriteLine();

            // 3. Execute - INSERT
            Console.WriteLine("--- Execute: 신규 상품 추가 ---");
            int affected = agent.Execute(
                "INSERT INTO Products (Name, Price) VALUES (@Name, @Price)",
                new { Name = "신제품D", Price = 4000 });
            Console.WriteLine($"  영향받은 행: {affected}개");

            Console.WriteLine();

            // 4. ExecuteInTransaction - 트랜잭션
            Console.WriteLine("--- ExecuteInTransaction: 가격 일괄 인상 ---");
            agent.ExecuteInTransaction((conn, tx) =>
            {
                return conn.Execute(
                    "UPDATE Products SET Price = Price + 500",
                    transaction: tx);
            });

            IEnumerable<Product> updated = agent.Query<Product>("SELECT Id, Name, Price FROM Products");
            foreach (Product p in updated)
                Console.WriteLine($"  [{p.Id}] {p.Name} - {p.Price:N0}원");

            Console.WriteLine();

            // 5. SetConnection - 경로 오류
            Console.WriteLine("--- SetConnection: 존재하지 않는 파일 ---");
            var agent2 = new DapperAgent();
            bool connected = agent2.SetConnection(@"C:\nonexistent\db.db");
            Console.WriteLine($"  연결 결과: {connected}");
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }

        Console.WriteLine();
        Console.WriteLine("=== Database 예제 종료 ===");
    }

    private static void SetupExampleDb(string dbPath)
    {
        using SqliteConnection conn = new SqliteConnection($"Data Source={dbPath}");
        conn.Open();
        using SqliteCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Products (
                Id    INTEGER PRIMARY KEY AUTOINCREMENT,
                Name  TEXT    NOT NULL,
                Price INTEGER NOT NULL
            );
            INSERT INTO Products (Name, Price) VALUES ('상품A', 1000);
            INSERT INTO Products (Name, Price) VALUES ('상품B', 2000);
            INSERT INTO Products (Name, Price) VALUES ('상품C', 3000);";
        cmd.ExecuteNonQuery();
    }
}
