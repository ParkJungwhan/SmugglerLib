using System.Security.Cryptography;
using System.Text;

namespace Smuggler.Common.Security;

public sealed class AesApiKeyCryptoService
{
    private readonly byte[] _key;

    public AesApiKeyCryptoService(string privateKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(privateKey);

        using SHA256 sha = SHA256.Create();
        _key = sha.ComputeHash(Encoding.UTF8.GetBytes(privateKey));
        ValidateAesKey(_key);
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

        return CombineIvAndCipher(aes.IV, cipherBytes);
    }

    public string EncryptToString(string plainText)
    {
        return Convert.ToBase64String(Encrypt(plainText));
    }

    public string DecryptToString(string cipherText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cipherText);
        return Decrypt(Convert.FromBase64String(cipherText));
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

    private static byte[] CombineIvAndCipher(byte[] iv, byte[] cipherBytes)
    {
        byte[] result = new byte[iv.Length + cipherBytes.Length];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, iv.Length, cipherBytes.Length);
        return result;
    }

    private static void ValidateAesKey(byte[] key)
    {
        if (key.Length is not (16 or 24 or 32))
        {
            throw new ArgumentException("AES key length must be 16, 24, or 32 bytes.", nameof(key));
        }
    }
}

public sealed class LegacyAesApiKeyCryptoService
{
    private readonly byte[] _legacyKey;

    public LegacyAesApiKeyCryptoService(string privateKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(privateKey);

        using SHA256 sha = SHA256.Create();
        _legacyKey = sha.ComputeHash(Encoding.UTF8.GetBytes(privateKey))[..8];
    }

    [Obsolete("Legacy AES helper retained only for compatibility. Prefer AesApiKeyCryptoService.")]
    public byte[] EncryptStringToBytes_AES(string plainText, byte[] key, byte[] iv)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);
        ValidateLegacyVector(key, nameof(key));
        ValidateLegacyVector(iv, nameof(iv));

        using RijndaelManaged rijndael = new();
        rijndael.Key = key;
        rijndael.IV = iv;

        using MemoryStream memoryStream = new();
        using ICryptoTransform transform = rijndael.CreateEncryptor(rijndael.Key, rijndael.IV);
        using CryptoStream cryptoStream = new(memoryStream, transform, CryptoStreamMode.Write);
        using StreamWriter writer = new(cryptoStream);
        writer.Write(plainText);
        writer.Flush();
        cryptoStream.FlushFinalBlock();
        return memoryStream.ToArray();
    }

    [Obsolete("Legacy AES helper retained only for compatibility. Prefer AesApiKeyCryptoService.")]
    public string DecryptBytesToString_AES(byte[] cipherText, byte[] key, byte[] iv)
    {
        ArgumentNullException.ThrowIfNull(cipherText);
        ValidateLegacyVector(key, nameof(key));
        ValidateLegacyVector(iv, nameof(iv));

        using RijndaelManaged rijndael = new();
        rijndael.Key = key;
        rijndael.IV = iv;
        using ICryptoTransform transform = rijndael.CreateDecryptor(rijndael.Key, rijndael.IV);
        using MemoryStream memoryStream = new(cipherText);
        using CryptoStream cryptoStream = new(memoryStream, transform, CryptoStreamMode.Read);
        using StreamReader reader = new(cryptoStream);
        return reader.ReadToEnd();
    }

    [Obsolete("Legacy DES helper retained only for compatibility. Prefer AesApiKeyCryptoService.")]
    public string EncryptString(string plainText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);

        using DESCryptoServiceProvider des = new();
        des.Mode = CipherMode.ECB;
        des.Padding = PaddingMode.PKCS7;
        des.Key = _legacyKey;
        des.IV = _legacyKey;

        using MemoryStream memoryStream = new();
        using CryptoStream cryptoStream = new(memoryStream, des.CreateEncryptor(), CryptoStreamMode.Write);
        byte[] bytes = Encoding.UTF8.GetBytes(plainText);
        cryptoStream.Write(bytes, 0, bytes.Length);
        cryptoStream.FlushFinalBlock();
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    [Obsolete("Legacy DES helper retained only for compatibility. Prefer AesApiKeyCryptoService.")]
    public string DecryptString(string cipherText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cipherText);

        using DESCryptoServiceProvider des = new();
        des.Mode = CipherMode.ECB;
        des.Padding = PaddingMode.PKCS7;
        des.Key = _legacyKey;
        des.IV = _legacyKey;

        using MemoryStream memoryStream = new();
        using CryptoStream cryptoStream = new(memoryStream, des.CreateDecryptor(), CryptoStreamMode.Write);
        byte[] buffer = Convert.FromBase64String(cipherText.Replace(" ", "+", StringComparison.Ordinal));
        cryptoStream.Write(buffer, 0, buffer.Length);
        cryptoStream.FlushFinalBlock();
        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    private static void ValidateLegacyVector(byte[] value, string paramName)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length == 0)
        {
            throw new ArgumentException("Legacy crypto vector must not be empty.", paramName);
        }
    }
}
