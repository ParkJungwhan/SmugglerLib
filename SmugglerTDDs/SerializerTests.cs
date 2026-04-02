using System;
using System.IO;
using MemoryPack;
using Smuggler.Common.DataProcess;
using Smuggler.Common.Interfaces;

namespace SmugglerTDDs;

/// <summary>
/// C0304: MemoryPackSerializerImpl 직렬화 round-trip 및 정책 검증 테스트
/// </summary>
public class SerializerTests : IDisposable
{
    private readonly ISerializer _serializer = new MemoryPackSerializerImpl();
    private readonly string _tempDir;

    public SerializerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"smug_ser_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // -------------------------------------------------------------------------
    // Round-trip 테스트
    // -------------------------------------------------------------------------

    [Fact]
    public void Serialize_Deserialize_Primitive_RoundTrip()
    {
        int original = 42;

        byte[] data = _serializer.Serialize(original);
        int result = _serializer.Deserialize<int>(data);

        Assert.Equal(original, result);
    }

    [Fact]
    public void Serialize_Deserialize_String_RoundTrip()
    {
        string original = "Hello, MemoryPack!";

        byte[] data = _serializer.Serialize(original);
        string result = _serializer.Deserialize<string>(data);

        Assert.Equal(original, result);
    }

    [Fact]
    public void Serialize_Deserialize_PacketObject_RoundTrip()
    {
        var original = new TestPacket { Id = 7, Name = "TestPlayer", Score = 9999 };

        byte[] data = _serializer.Serialize(original);
        TestPacket result = _serializer.Deserialize<TestPacket>(data);

        Assert.Equal(original.Id, result.Id);
        Assert.Equal(original.Name, result.Name);
        Assert.Equal(original.Score, result.Score);
    }

    [Fact]
    public void Serialize_NullReference_ReturnsNonEmptyBytes()
    {
        // null 직렬화는 허용. MemoryPack은 null을 표현하는 바이트를 생성한다.
        byte[] data = _serializer.Serialize<TestPacket?>(null);

        Assert.NotNull(data);
        Assert.True(data.Length > 0);
    }

    // -------------------------------------------------------------------------
    // C0301: null / 빈 배열 입력 정책 검증
    // -------------------------------------------------------------------------

    [Fact]
    public void Deserialize_NullData_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _serializer.Deserialize<TestPacket>(null!));
    }

    [Fact]
    public void Deserialize_EmptyData_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _serializer.Deserialize<TestPacket>(Array.Empty<byte>()));
    }

    // -------------------------------------------------------------------------
    // C0302: 예외 메시지 표준화 검증
    // -------------------------------------------------------------------------

    [Fact]
    public void Deserialize_NullData_ExceptionMessageContainsParamName()
    {
        // ArgumentException 메시지에 파라미터 이름이 포함되는지 확인
        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => _serializer.Deserialize<TestPacket>(null!));

        Assert.Contains("data", ex.Message);
    }

    [Fact]
    public void Serialize_Deserialize_LargePayload_RoundTrip()
    {
        // 큰 데이터에서도 직렬화/역직렬화가 정상 동작하는지 확인
        var original = new TestPacket { Id = 99, Name = new string('X', 10_000), Score = int.MaxValue };

        byte[] data = _serializer.Serialize(original);
        TestPacket result = _serializer.Deserialize<TestPacket>(data);

        Assert.Equal(original.Id, result.Id);
        Assert.Equal(original.Name, result.Name);
        Assert.Equal(original.Score, result.Score);
    }

    // -------------------------------------------------------------------------
    // C0303: 파일 저장/로드 round-trip 테스트
    // -------------------------------------------------------------------------

    [Fact]
    public void SaveToFile_LoadFromFile_RoundTrip()
    {
        var original = new TestPacket { Id = 1, Name = "FileTest", Score = 500 };
        string filePath = Path.Combine(_tempDir, "packet.bin");

        _serializer.SaveToFile(original, filePath);
        TestPacket loaded = _serializer.LoadFromFile<TestPacket>(filePath);

        Assert.Equal(original.Id, loaded.Id);
        Assert.Equal(original.Name, loaded.Name);
        Assert.Equal(original.Score, loaded.Score);
    }

    [Fact]
    public void SaveToFile_CreatesDirectoryIfNotExists()
    {
        string filePath = Path.Combine(_tempDir, "subdir", "nested", "packet.bin");

        _serializer.SaveToFile(new TestPacket { Id = 1 }, filePath);

        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void LoadFromFile_NonExistentFile_ThrowsFileNotFoundException()
    {
        string filePath = Path.Combine(_tempDir, "ghost.bin");

        Assert.Throws<FileNotFoundException>(() => _serializer.LoadFromFile<TestPacket>(filePath));
    }

    [Fact]
    public void SaveToFile_EmptyFilePath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _serializer.SaveToFile(new TestPacket(), ""));
    }

    [Fact]
    public void LoadFromFile_EmptyFilePath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _serializer.LoadFromFile<TestPacket>(""));
    }
}

// -------------------------------------------------------------------------
// 테스트용 MemoryPackable 타입
// -------------------------------------------------------------------------

[MemoryPackable]
public partial class TestPacket
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int Score { get; set; }
}
