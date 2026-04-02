using System.Security.Cryptography;
using System.Text;

namespace Smuggler.Common.Security;

/// <summary>
/// AesApiKeyCryptoService의 레거시 호환 메서드 계층.<br/>
/// 이 파일의 메서드는 .NET 6 이상에서 제거된 RijndaelManaged / DESCryptoServiceProvider API를 사용하므로
/// 컴파일은 되지만 런타임에서 PlatformNotSupportedException이 발생할 수 있다.<br/>
/// 새 코드에서는 Encrypt / Decrypt / EncryptToString / DecryptToString을 사용할 것.
/// </summary>
public sealed partial class AesApiKeyCryptoService
{
#pragma warning disable SYSLIB0022 // RijndaelManaged is obsolete
#pragma warning disable SYSLIB0021 // DESCryptoServiceProvider is obsolete

    /// <inheritdoc cref="EncryptStringToBytes_AES_Legacy"/>
    [Obsolete("RijndaelManaged는 .NET 6 이상에서 지원이 제한됩니다. Encrypt(string)을 사용하세요.")]
    public byte[] EncryptStringToBytes_AES(string plainText, byte[] key, byte[] IV)
    {
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException(nameof(plainText));
        if (key == null || key.Length == 0)
            throw new ArgumentNullException(nameof(key));
        if (IV == null || IV.Length == 0)
            throw new ArgumentNullException(nameof(IV));

        MemoryStream? memoryStream = null;
        CryptoStream? cryptoStream = null;
        StreamWriter? streamWriter = null;
        RijndaelManaged? rijndaelManaged = null;
        try
        {
            rijndaelManaged = new RijndaelManaged();
            rijndaelManaged.Key = key;
            rijndaelManaged.IV = IV;
            ICryptoTransform transform = rijndaelManaged.CreateEncryptor(rijndaelManaged.Key, rijndaelManaged.IV);
            memoryStream = new MemoryStream();
            cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
            streamWriter = new StreamWriter(cryptoStream);
            streamWriter.Write(plainText);
        }
        finally
        {
            streamWriter?.Close();
            cryptoStream?.Close();
            memoryStream?.Close();
            rijndaelManaged?.Clear();
        }

        return memoryStream.ToArray();
    }

    /// <inheritdoc cref="DecryptBytesToString_AES_Legacy"/>
    [Obsolete("RijndaelManaged는 .NET 6 이상에서 지원이 제한됩니다. Decrypt(byte[])를 사용하세요.")]
    public string DecryptBytesToString_AES(byte[] cipherText, byte[] key, byte[] IV)
    {
        if (cipherText == null || cipherText.Length == 0)
            throw new ArgumentNullException(nameof(cipherText));
        if (key == null || key.Length == 0)
            throw new ArgumentNullException(nameof(key));
        if (IV == null || IV.Length == 0)
            throw new ArgumentNullException(nameof(IV));

        MemoryStream? memoryStream = null;
        CryptoStream? cryptoStream = null;
        StreamReader? streamReader = null;
        RijndaelManaged? rijndaelManaged = null;
        try
        {
            rijndaelManaged = new RijndaelManaged();
            rijndaelManaged.Key = key;
            rijndaelManaged.IV = IV;
            ICryptoTransform transform = rijndaelManaged.CreateDecryptor(rijndaelManaged.Key, rijndaelManaged.IV);
            memoryStream = new MemoryStream(cipherText);
            cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
            streamReader = new StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        }
        finally
        {
            streamReader?.Close();
            cryptoStream?.Close();
            memoryStream?.Close();
            rijndaelManaged?.Clear();
        }
    }

    [Obsolete("DESCryptoServiceProvider는 .NET 6 이상에서 지원이 제한됩니다. EncryptToString(string)을 사용하세요.")]
    public string EncryptString(string strKey)
    {
        DESCryptoServiceProvider des = new DESCryptoServiceProvider();
        des.Mode = CipherMode.ECB;
        des.Padding = PaddingMode.PKCS7;
        des.Key = pbyteKey;
        des.IV = pbyteKey;
        MemoryStream memoryStream = new MemoryStream();
        CryptoStream cryptoStream = new CryptoStream(memoryStream, des.CreateEncryptor(), CryptoStreamMode.Write);
        byte[] bytes = Encoding.UTF8.GetBytes(strKey.ToCharArray());
        cryptoStream.Write(bytes, 0, bytes.Length);
        cryptoStream.FlushFinalBlock();
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    [Obsolete("DESCryptoServiceProvider는 .NET 6 이상에서 지원이 제한됩니다. DecryptToString(string)을 사용하세요.")]
    public string DecryptString(string strKey)
    {
        DESCryptoServiceProvider des = new DESCryptoServiceProvider();
        des.Mode = CipherMode.ECB;
        des.Padding = PaddingMode.PKCS7;
        des.Key = pbyteKey;
        des.IV = pbyteKey;
        MemoryStream memoryStream = new MemoryStream();
        CryptoStream cryptoStream = new CryptoStream(memoryStream, des.CreateDecryptor(), CryptoStreamMode.Write);
        strKey = strKey.Replace(" ", "+");
        byte[] array = Convert.FromBase64String(strKey);
        cryptoStream.Write(array, 0, array.Length);
        cryptoStream.FlushFinalBlock();
        string result = Encoding.UTF8.GetString(memoryStream.GetBuffer());
        return result.Replace("\0", "");
    }

#pragma warning restore SYSLIB0022
#pragma warning restore SYSLIB0021
}
