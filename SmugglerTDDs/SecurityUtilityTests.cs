using System.Security.Cryptography;
using Smuggler.Common.Security;

namespace SmugglerTDDs;

public class SecurityUtilityTests
{
    [Fact]
    public void AesApiKeyCryptoService_RoundTrips_StringPayload()
    {
        AesApiKeyCryptoService crypto = new("platform-secret");

        string cipherText = crypto.EncryptToString("hello-smuggler");
        string plainText = crypto.DecryptToString(cipherText);

        Assert.Equal("hello-smuggler", plainText);
    }

    [Fact]
    public void AesEncryption_Validates_KeyLength()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new AESEncryption("short-key"));

        Assert.Contains("16, 24, or 32 bytes", exception.Message);
    }

    [Fact]
    public void AesEncryption_RoundTrips_BytesPayload()
    {
        AESEncryption aes = new("1234567890ABCDEF");

        byte[] cipher = aes.Encrypt("payload-01");
        string plain = aes.Decrypt(cipher);

        Assert.Equal("payload-01", plain);
    }

    [Fact]
    public void LegacyAesApiKeyCryptoService_RoundTrips_CompatibilityPayload()
    {
        LegacyAesApiKeyCryptoService legacy = new("legacy-secret");

        string cipher = legacy.EncryptString("compatibility");
        string plain = legacy.DecryptString(cipher);

        Assert.Equal("compatibility", plain);
    }

    [Fact]
    public void RsaCrypto_RoundTrips_WithXmlKeys()
    {
        RSACrypto rsaCrypto = new();
        (string publicKeyXml, string privateKeyXml) = rsaCrypto.CreateXmlKeyPair();

        string cipher = rsaCrypto.RSAEncrypt("xml-payload", publicKeyXml);
        string plain = rsaCrypto.RSADecrypt(cipher, privateKeyXml);

        Assert.Equal("xml-payload", plain);
    }

    [Fact]
    public void RsaCrypto_RoundTrips_WithPemKeys()
    {
        RSACrypto rsaCrypto = new();
        (string publicKeyPem, string privateKeyPem) = rsaCrypto.CreatePemKeyPair();

        string cipher = rsaCrypto.RSAEncrypt("pem-payload", publicKeyPem);
        string plain = rsaCrypto.RSADecrypt(cipher, privateKeyPem);

        Assert.Equal("pem-payload", plain);
    }

    [Fact]
    public void RsaCrypto_Throws_ForInvalidKeyFormat()
    {
        RSACrypto rsaCrypto = new();

        ArgumentException exception = Assert.Throws<ArgumentException>(() => rsaCrypto.RSAEncrypt("test", "not-a-key"));

        Assert.Contains("valid XML key or PEM-formatted key", exception.Message);
    }

    [Fact]
    public void Md5Crypt_CreatesStableChecksum()
    {
        string checksum = MD5Crypt.CreateChecksum("checksum-input");

        Assert.Equal(32, checksum.Length);
        Assert.Equal(checksum, MD5Crypt.CreateChecksum("checksum-input"));
    }
}
