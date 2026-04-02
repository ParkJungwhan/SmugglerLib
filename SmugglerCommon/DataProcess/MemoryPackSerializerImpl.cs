using MemoryPack;
using Smuggler.Common.Interfaces;

namespace Smuggler.Common.DataProcess;

public class MemoryPackSerializerImpl : ISerializer
{
    public byte[] Serialize<T>(T? obj)
    {
        if (obj is null)
        {
            return [];
        }

        try
        {
            return MemoryPackSerializer.Serialize(obj);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"MemoryPack serialization failed for type '{typeof(T).FullName}'.", exception);
        }
    }

    public T? Deserialize<T>(byte[]? data)
    {
        if (data is null || data.Length == 0)
        {
            return default;
        }

        try
        {
            return MemoryPackSerializer.Deserialize<T>(data);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"MemoryPack deserialization failed for type '{typeof(T).FullName}'.", exception);
        }
    }

    public void SerializeToStream<T>(Stream stream, T? obj)
    {
        ArgumentNullException.ThrowIfNull(stream);

        byte[] bytes = Serialize(obj);
        stream.Write(bytes, 0, bytes.Length);
    }

    public T? DeserializeFromStream<T>(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using MemoryStream buffer = new();
        stream.CopyTo(buffer);
        return Deserialize<T>(buffer.ToArray());
    }

    public void SaveToFile<T>(string filePath, T? obj)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        string fullPath = Path.GetFullPath(filePath);
        string? directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes(fullPath, Serialize(obj));
    }

    public T? LoadFromFile<T>(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        string fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Serialized file does not exist: '{fullPath}'.", fullPath);
        }

        return Deserialize<T>(File.ReadAllBytes(fullPath));
    }
}
