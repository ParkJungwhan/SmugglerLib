namespace Smuggler.Common.Interfaces;

public interface ISerializer
{
    byte[] Serialize<T>(T? obj);

    T? Deserialize<T>(byte[]? data);
}
