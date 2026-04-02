using MemoryPack;
using Smuggler.Common.DataProcess;

namespace SmugglerTDDs;

public sealed class MemoryPackSerializerTests : IDisposable
{
    private readonly string _filePath;
    private readonly MemoryPackSerializerImpl _serializer = new();

    public MemoryPackSerializerTests()
    {
        _filePath = Path.Combine(Path.GetTempPath(), $"smuggler-serializer-{Guid.NewGuid():N}.bin");
    }

    [Fact]
    public void SerializeAndDeserialize_RoundTripSamplePacket()
    {
        SamplePacket packet = new()
        {
            Id = 7,
            Name = "merchant",
            Values = [10, 20, 30],
        };

        byte[] bytes = _serializer.Serialize(packet);
        SamplePacket? restored = _serializer.Deserialize<SamplePacket>(bytes);

        Assert.NotNull(restored);
        Assert.Equal(packet.Id, restored!.Id);
        Assert.Equal(packet.Name, restored.Name);
        Assert.Equal(packet.Values, restored.Values);
    }

    [Fact]
    public void Serialize_ReturnsEmptyBytes_WhenInputIsNull()
    {
        byte[] bytes = _serializer.Serialize<SamplePacket>(null);

        Assert.Empty(bytes);
    }

    [Fact]
    public void Deserialize_ReturnsDefault_WhenInputIsNullOrEmpty()
    {
        SamplePacket? fromNull = _serializer.Deserialize<SamplePacket>(null);
        SamplePacket? fromEmpty = _serializer.Deserialize<SamplePacket>([]);

        Assert.Null(fromNull);
        Assert.Null(fromEmpty);
    }

    [Fact]
    public void SaveToFileAndLoadFromFile_RoundTripSamplePacket()
    {
        SamplePacket packet = new()
        {
            Id = 99,
            Name = "courier",
            Values = [1, 2, 3],
        };

        _serializer.SaveToFile(_filePath, packet);
        SamplePacket? restored = _serializer.LoadFromFile<SamplePacket>(_filePath);

        Assert.NotNull(restored);
        Assert.Equal(packet.Id, restored!.Id);
        Assert.Equal(packet.Name, restored.Name);
        Assert.Equal(packet.Values, restored.Values);
    }

    [Fact]
    public void SerializeToStreamAndDeserializeFromStream_RoundTripSamplePacket()
    {
        SamplePacket packet = new()
        {
            Id = 11,
            Name = "smuggler",
            Values = [4, 5],
        };

        using MemoryStream stream = new();
        _serializer.SerializeToStream(stream, packet);
        stream.Position = 0;

        SamplePacket? restored = _serializer.DeserializeFromStream<SamplePacket>(stream);

        Assert.NotNull(restored);
        Assert.Equal(packet.Id, restored!.Id);
        Assert.Equal(packet.Name, restored.Name);
    }

    [Fact]
    public void Deserialize_ThrowsStandardizedMessage_WhenPayloadIsInvalid()
    {
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            _serializer.Deserialize<SamplePacket>([1, 2, 3, 4]));

        Assert.Contains("MemoryPack deserialization failed", exception.Message);
        Assert.Contains(typeof(SamplePacket).FullName!, exception.Message);
    }

    public void Dispose()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }
}

[MemoryPackable]
public partial class SamplePacket
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<int> Values { get; set; } = [];
}
