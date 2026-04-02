using System.Security.Cryptography;
using System.Text;

namespace Smuggler.Common.Security;

public class RSACrypto
{
    public string RSAEncrypt(string plainText, string publicKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);
        ArgumentException.ThrowIfNullOrWhiteSpace(publicKey);

        using RSA rsa = CreateRsaFromKey(publicKey, includePrivate: false);
        byte[] input = Encoding.UTF8.GetBytes(plainText);
        byte[] cipher = rsa.Encrypt(input, RSAEncryptionPadding.OaepSHA256);
        return Convert.ToBase64String(cipher);
    }

    public string RSADecrypt(string cipherText, string privateKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cipherText);
        ArgumentException.ThrowIfNullOrWhiteSpace(privateKey);

        using RSA rsa = CreateRsaFromKey(privateKey, includePrivate: true);
        byte[] input = Convert.FromBase64String(cipherText);
        byte[] plain = rsa.Decrypt(input, RSAEncryptionPadding.OaepSHA256);
        return Encoding.UTF8.GetString(plain);
    }

    public (string PublicKeyPem, string PrivateKeyPem) CreatePemKeyPair(int keySize = 2048)
    {
        using RSA rsa = RSA.Create(keySize);
        return (rsa.ExportRSAPublicKeyPem(), rsa.ExportPkcs8PrivateKeyPem());
    }

    public (string PublicKeyXml, string PrivateKeyXml) CreateXmlKeyPair(int keySize = 2048)
    {
        using RSA rsa = RSA.Create(keySize);
        return (rsa.ToXmlString(false), rsa.ToXmlString(true));
    }

    private static RSA CreateRsaFromKey(string keyText, bool includePrivate)
    {
        RSA rsa = RSA.Create();
        string trimmed = keyText.Trim();

        if (trimmed.StartsWith("<RSAKeyValue>", StringComparison.Ordinal))
        {
            rsa.FromXmlString(trimmed);
            return rsa;
        }

        try
        {
            if (includePrivate)
            {
                rsa.ImportFromPem(trimmed);
            }
            else
            {
                rsa.ImportFromPem(trimmed);
            }

            return rsa;
        }
        catch (Exception exception)
        {
            rsa.Dispose();
            throw new ArgumentException("RSA key must be a valid XML key or PEM-formatted key.", nameof(keyText), exception);
        }
    }
}
