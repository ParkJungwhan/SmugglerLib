using MemoryPack;
using Smuggler.Common.DataProcess;

namespace TestConsole;

public static class SerializationExample
{
    public static void Run()
    {
        MemoryPackSerializerImpl serializer = new();
        string dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDirectory);

        string filePath = Path.Combine(dataDirectory, "sample-packet.bin");
        SerializationExamplePacket packet = new()
        {
            Id = 42,
            Name = "trade-packet",
            Values = [100, 200, 300],
        };

        byte[] bytes = serializer.Serialize(packet);
        SerializationExamplePacket? restored = serializer.Deserialize<SerializationExamplePacket>(bytes);

        serializer.SaveToFile(filePath, packet);
        SerializationExamplePacket? loaded = serializer.LoadFromFile<SerializationExamplePacket>(filePath);

        Console.WriteLine($"Serialized bytes: {bytes.Length}");
        Console.WriteLine($"Round-trip packet: {restored?.Id} / {restored?.Name} / {string.Join(',', restored?.Values ?? [])}");
        Console.WriteLine($"Loaded from file: {loaded?.Id} / {loaded?.Name}");
    }
}

[MemoryPackable]
public partial class SerializationExamplePacket
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<int> Values { get; set; } = [];
}
