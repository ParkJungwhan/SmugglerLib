using System.Security.Cryptography;
using System.Text;

namespace Smuggler.Common.Security;

/// <summary>
/// 문자열 키 기반 AES-CBC 암복호화 유틸리티.<br/>
/// <b>키 길이 정책:</b> stringkey를 ASCII 인코딩한 바이트 수가 16 / 24 / 32 이어야 한다.
/// </summary>
public class AESEncryption
{
    private readonly byte[] key;

    /// <exception cref="ArgumentException">
    /// stringkey가 null/비어 있거나, ASCII 인코딩 시 길이가 16/24/32 바이트가 아닐 때.
    /// </exception>
    public AESEncryption(string stringkey)
    {
        if (string.IsNullOrEmpty(stringkey))
            throw new ArgumentException("AES 키가 비어 있습니다.", nameof(stringkey));

        this.key = Encoding.ASCII.GetBytes(stringkey);

        if (this.key.Length != 16 && this.key.Length != 24 && this.key.Length != 32)
            throw new ArgumentException(
                $"AES 키 길이는 16, 24, 32 바이트여야 합니다. 현재: {this.key.Length}바이트 (입력 문자 수 기준)",
                nameof(stringkey));
    }

    /// <summary>
    /// plainText를 AES-CBC로 암호화하고 [IV(16) + 암호문] 바이트 배열을 반환한다.
    /// </summary>
    /// <exception cref="ArgumentException">plainText가 null이거나 비어 있을 때.</exception>
    public byte[] Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("암호화할 문자열이 비어 있습니다.", nameof(plainText));

        using Aes aes = Aes.Create();
        aes.Key = this.key;
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

    /// <summary>
    /// [IV(16) + 암호문] 바이트 배열을 AES-CBC로 복호화하여 원문을 반환한다.
    /// </summary>
    /// <exception cref="ArgumentException">encryptedData가 null이거나 17바이트 미만일 때.</exception>
    public string Decrypt(byte[] encryptedData)
    {
        if (encryptedData is null || encryptedData.Length <= 16)
            throw new ArgumentException(
                $"암호화된 데이터가 유효하지 않습니다. 최소 17바이트 이상이어야 합니다. 현재: {encryptedData?.Length ?? 0}바이트",
                nameof(encryptedData));

        using Aes aes = Aes.Create();
        aes.Key = this.key;
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
