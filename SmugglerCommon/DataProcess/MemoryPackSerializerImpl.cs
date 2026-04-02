using MemoryPack;
using Smuggler.Common.Interfaces;

namespace Smuggler.Common.DataProcess;

/// <summary>
/// MemoryPack 기반 직렬화 구현체.
/// </summary>
public class MemoryPackSerializerImpl : ISerializer
{
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">직렬화 중 오류가 발생했을 때.</exception>
    public byte[] Serialize<T>(T obj)
    {
        try
        {
            return MemoryPackSerializer.Serialize(obj);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"직렬화 실패 (타입: {typeof(T).Name}): {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">data가 null이거나 빈 배열일 때.</exception>
    /// <exception cref="InvalidOperationException">역직렬화 중 오류가 발생했을 때.</exception>
    public T Deserialize<T>(byte[] data)
    {
        if (data is null || data.Length == 0)
            throw new ArgumentException("역직렬화할 데이터가 null이거나 비어 있습니다.", nameof(data));

        try
        {
            return MemoryPackSerializer.Deserialize<T>(data)!;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"역직렬화 실패 (타입: {typeof(T).Name}): {ex.Message}", ex);
        }
    }
}
