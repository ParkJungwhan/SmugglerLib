using System;
using System.IO;
using MemoryPack;
using Smuggler.Common.DataProcess;
using Smuggler.Common.Interfaces;

namespace TestConsole;

// -------------------------------------------------------------------------
// 샘플 패킷 정의
// -------------------------------------------------------------------------

/// <summary>플레이어 로그인 요청 패킷 샘플.</summary>
[MemoryPackable]
public partial class LoginRequestPacket
{
    public int PlayerId { get; set; }
    public string? UserId { get; set; }
    public string? Token { get; set; }
    public long Timestamp { get; set; }
}

/// <summary>플레이어 상태 정보 패킷 샘플.</summary>
[MemoryPackable]
public partial class PlayerStatePacket
{
    public int PlayerId { get; set; }
    public string? Name { get; set; }
    public int Level { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}

// -------------------------------------------------------------------------
// C0305: 직렬화 예제
// -------------------------------------------------------------------------

/// <summary>
/// C0305: MemoryPackSerializerImpl 직렬화 사용 예제
/// </summary>
internal static class Serializer
{
    internal static void Run()
    {
        Console.WriteLine("=== Serializer 예제 시작 ===");
        Console.WriteLine();

        ISerializer serializer = new MemoryPackSerializerImpl();

        // 1. 기본 직렬화 / 역직렬화
        Console.WriteLine("--- LoginRequestPacket round-trip ---");
        var loginReq = new LoginRequestPacket
        {
            PlayerId  = 1001,
            UserId    = "player_hong",
            Token     = "eyJhbGciOiJIUzI1NiJ9.sample",
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };
        byte[] loginBytes = serializer.Serialize(loginReq);
        LoginRequestPacket restored = serializer.Deserialize<LoginRequestPacket>(loginBytes);
        Console.WriteLine($"  원본:   PlayerId={loginReq.PlayerId}, UserId={loginReq.UserId}");
        Console.WriteLine($"  복원:   PlayerId={restored.PlayerId}, UserId={restored.UserId}");
        Console.WriteLine($"  바이트: {loginBytes.Length}bytes");

        Console.WriteLine();

        // 2. 파일 저장 / 로드
        Console.WriteLine("--- PlayerStatePacket 파일 저장/로드 ---");
        string tempFile = Path.Combine(Path.GetTempPath(), "player_state.bin");
        var state = new PlayerStatePacket
        {
            PlayerId = 1001,
            Name     = "홍길동",
            Level    = 75,
            X        = 128.5f,
            Y        = 0.0f,
            Z        = -64.3f,
        };

        serializer.SaveToFile(state, tempFile);
        PlayerStatePacket loaded = serializer.LoadFromFile<PlayerStatePacket>(tempFile);
        Console.WriteLine($"  저장 경로: {tempFile}");
        Console.WriteLine($"  복원: Name={loaded.Name}, Level={loaded.Level}, Pos=({loaded.X}, {loaded.Y}, {loaded.Z})");
        File.Delete(tempFile);

        Console.WriteLine();

        // 3. null 직렬화 허용 확인
        Console.WriteLine("--- null 직렬화 ---");
        byte[] nullBytes = serializer.Serialize<LoginRequestPacket?>(null);
        Console.WriteLine($"  null 직렬화 바이트 수: {nullBytes.Length}");

        Console.WriteLine();

        // 4. 입력 오류 케이스
        Console.WriteLine("--- 빈 배열 역직렬화 예외 ---");
        try
        {
            serializer.Deserialize<LoginRequestPacket>(Array.Empty<byte>());
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"  ArgumentException: {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("=== Serializer 예제 종료 ===");
    }
}
