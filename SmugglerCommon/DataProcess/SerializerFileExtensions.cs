using Smuggler.Common.Interfaces;

namespace Smuggler.Common.DataProcess;

/// <summary>
/// ISerializer에 파일 기반 저장/로드 기능을 추가하는 확장 메서드 모음.
/// </summary>
public static class SerializerFileExtensions
{
    /// <summary>
    /// obj를 직렬화하여 파일로 저장한다. 파일이 이미 존재하면 덮어쓴다.
    /// </summary>
    /// <param name="filePath">저장할 파일 경로. 디렉터리가 없으면 자동 생성된다.</param>
    /// <exception cref="ArgumentException">filePath가 null이거나 비어 있을 때.</exception>
    public static void SaveToFile<T>(this ISerializer serializer, T obj, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("파일 경로가 비어 있습니다.", nameof(filePath));

        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        byte[] data = serializer.Serialize(obj);
        File.WriteAllBytes(filePath, data);
    }

    /// <summary>
    /// 파일을 읽어 T로 역직렬화한다.
    /// </summary>
    /// <param name="filePath">읽을 파일 경로.</param>
    /// <exception cref="ArgumentException">filePath가 null이거나 비어 있을 때.</exception>
    /// <exception cref="FileNotFoundException">파일이 존재하지 않을 때.</exception>
    public static T LoadFromFile<T>(this ISerializer serializer, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("파일 경로가 비어 있습니다.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"직렬화 파일을 찾을 수 없습니다: {filePath}", filePath);

        byte[] data = File.ReadAllBytes(filePath);
        return serializer.Deserialize<T>(data);
    }
}
