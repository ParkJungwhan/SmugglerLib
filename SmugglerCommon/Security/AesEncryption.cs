using System.Security.Cryptography;
using System.Text;

namespace Smuggler.Common.Security;

public class AESEncryption
{
    private readonly byte[] _key;

    public AESEncryption(string stringKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stringKey);

        _key = Encoding.UTF8.GetBytes(stringKey);
        if (_key.Length is not (16 or 24 or 32))
        {
            throw new ArgumentException("AES key must be exactly 16, 24, or 32 bytes when encoded as UTF-8.", nameof(stringKey));
        }
    }

    public byte[] Encrypt(string plainText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);

        using Aes aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        using ICryptoTransform encryptor = aes.CreateEncryptor();
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        byte[] result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
        return result;
    }

    public string Decrypt(byte[] encryptedData)
    {
        ArgumentNullException.ThrowIfNull(encryptedData);

        if (encryptedData.Length <= 16)
        {
            throw new ArgumentException("Encrypted payload must contain a 16-byte IV and cipher text.", nameof(encryptedData));
        }

        using Aes aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        byte[] iv = encryptedData[..16];
        byte[] cipherText = encryptedData[16..];
        aes.IV = iv;

        using ICryptoTransform decryptor = aes.CreateDecryptor();
        byte[] plainBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
