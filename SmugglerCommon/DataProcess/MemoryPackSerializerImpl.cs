using MemoryPack;
using Smuggler.Common.Interfaces;

namespace Smuggler.Common.DataProcess;

public class MemoryPackSerializerImpl : ISerializer
{
    public byte[] Serialize<T>(T obj)
    {
        return MemoryPackSerializer.Serialize(obj);
    }

    public T Deserialize<T>(byte[] data)
    {
        return MemoryPackSerializer.Deserialize<T>(data);
    }
}