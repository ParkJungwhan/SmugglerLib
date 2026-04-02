using System.Collections.Generic;
using System.Data;

namespace Smuggler.Common.DataBase;

/// <summary>
/// DB 에이전트의 최소 계약을 정의하는 인터페이스.
/// </summary>
public interface IDBAgent
{
    /// <summary>
    /// DB 파일 경로로 연결을 설정하고 연결 유효성을 확인한다.
    /// </summary>
    /// <param name="dbFilePath">SQLite DB 파일 경로.</param>
    /// <returns>연결 성공 시 true, 파일이 존재하지 않으면 false.</returns>
    bool SetConnection(string dbFilePath);

    /// <summary>sql을 실행하고 결과를 T 컬렉션으로 반환한다.</summary>
    IEnumerable<T> Query<T>(string sql, object? param = null);

    /// <summary>sql을 실행하고 단일 결과를 반환한다. 결과가 없으면 default(T)를 반환한다.</summary>
    T? QuerySingle<T>(string sql, object? param = null);

    /// <summary>sql을 실행하고 영향받은 행 수를 반환한다.</summary>
    int Execute(string sql, object? param = null);

    /// <summary>
    /// 트랜잭션 내에서 work를 실행한다.<br/>
    /// work가 예외를 던지면 롤백하고 예외를 다시 던진다.
    /// </summary>
    int ExecuteInTransaction(Func<IDbConnection, IDbTransaction, int> work);
}
