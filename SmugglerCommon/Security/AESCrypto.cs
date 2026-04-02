using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Smuggler.Common.Security;

/// <summary>
/// SHA-256 해시 기반 AES-256-CBC 암복호화 서비스.<br/>
/// 레거시 메서드는 <c>AesApiKeyCryptoServiceLegacy.cs</c>에 분리되어 있다.
/// </summary>
public sealed partial class AesApiKeyCryptoService
{
    private readonly byte[] pbyteKey;

    /// <summary>
    /// privatekey를 SHA-256으로 해시하여 AES-256 키를 생성한다.
    /// </summary>
    /// <exception cref="ArgumentException">privatekey가 null이거나 비어 있을 때.</exception>
    public AesApiKeyCryptoService(string privatekey)
    {
        if (string.IsNullOrEmpty(privatekey))
            throw new ArgumentException("AES 키가 비어 있습니다.", nameof(privatekey));

        using var sha = SHA256.Create();
        pbyteKey = sha.ComputeHash(Encoding.UTF8.GetBytes(privatekey));
    }

    /// <summary>
    /// plainText를 AES-256-CBC로 암호화하고 [IV(16) + 암호문] 바이트 배열을 반환한다.
    /// </summary>
    /// <exception cref="ArgumentException">plainText가 null이거나 비어 있을 때.</exception>
    public byte[] Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("암호화할 문자열이 비어 있습니다.", nameof(plainText));

        using Aes aes = Aes.Create();
        aes.Key = pbyteKey;
        aes.GenerateIV();
        aes.Mode = CipherMode.CBC;

        using var encryptor = aes.CreateEncryptor();
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        byte[] result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
        return result;
    }

    /// <summary>Encrypt 결과를 Base64 문자열로 반환한다.</summary>
    /// <exception cref="ArgumentException">plainText가 null이거나 비어 있을 때.</exception>
    public string EncryptToString(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("암호화할 문자열이 비어 있습니다.", nameof(plainText));

        return Convert.ToBase64String(Encrypt(plainText));
    }

    /// <summary>Base64 암호문 문자열을 복호화하여 원문을 반환한다.</summary>
    /// <exception cref="ArgumentException">plainText가 null이거나 비어 있을 때.</exception>
    public string DecryptToString(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("복호화할 문자열이 비어 있습니다.", nameof(plainText));

        return Decrypt(Convert.FromBase64String(plainText));
    }

    /// <summary>
    /// [IV(16) + 암호문] 바이트 배열을 AES-256-CBC로 복호화하여 원문을 반환한다.
    /// </summary>
    /// <exception cref="ArgumentException">encryptedData가 null이거나 17바이트 미만일 때.</exception>
    public string Decrypt(byte[] encryptedData)
    {
        if (encryptedData is null || encryptedData.Length <= 16)
            throw new ArgumentException(
                $"암호화된 데이터가 유효하지 않습니다. 최소 17바이트 이상이어야 합니다. 현재: {encryptedData?.Length ?? 0}바이트",
                nameof(encryptedData));

        using Aes aes = Aes.Create();
        aes.Key = pbyteKey;
        aes.Mode = CipherMode.CBC;

        byte[] iv = new byte[16];
        byte[] cipherText = new byte[encryptedData.Length - 16];
        Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(encryptedData, iv.Length, cipherText, 0, cipherText.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        byte[] plainBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
