namespace Smuggler.Common.Interfaces;

/// <summary>
/// 직렬화/역직렬화 계약을 정의하는 인터페이스.
/// </summary>
/// <remarks>
/// <b>null 입력 정책:</b><br/>
/// - Serialize: null 허용. 구현체는 null을 빈 표현(예: 빈 헤더)으로 직렬화할 수 있다.<br/>
/// - Deserialize: null 또는 빈 배열 입력 시 <see cref="ArgumentException"/>을 던진다.<br/>
/// <br/>
/// <b>예외 정책:</b><br/>
/// - 직렬화/역직렬화 내부 오류는 <see cref="InvalidOperationException"/>으로 래핑하여 던진다.<br/>
/// - 예외 메시지에는 대상 타입 이름과 원인 메시지가 포함되어야 한다.
/// </remarks>
public interface ISerializer
{
    /// <summary>
    /// obj를 바이트 배열로 직렬화한다.<br/>
    /// T가 참조 타입인 경우 null을 전달할 수 있다.
    /// </summary>
    byte[] Serialize<T>(T obj);

    /// <summary>
    /// 바이트 배열을 T로 역직렬화한다.
    /// </summary>
    /// <exception cref="ArgumentException">data가 null이거나 빈 배열일 때.</exception>
    /// <exception cref="InvalidOperationException">역직렬화 중 오류가 발생했을 때.</exception>
    T Deserialize<T>(byte[] data);
}
