using System;
using System.Security.Cryptography;
using Smuggler.Common.Security;

namespace SmugglerTDDs;

/// <summary>
/// C0405: AES / RSA / MD5 보안 유틸 테스트
/// </summary>
public class SecurityTests
{
    // =========================================================================
    // AesApiKeyCryptoService (AESCrypto)
    // =========================================================================

    [Fact]
    public void AesCrypto_Constructor_EmptyKey_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new AesApiKeyCryptoService(""));
    }

    [Fact]
    public void AesCrypto_Constructor_NullKey_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new AesApiKeyCryptoService(null!));
    }

    [Fact]
    public void AesCrypto_Encrypt_EmptyPlainText_ThrowsArgumentException()
    {
        var svc = new AesApiKeyCryptoService("testkey");
        Assert.Throws<ArgumentException>(() => svc.Encrypt(""));
    }

    [Fact]
    public void AesCrypto_Decrypt_TooShortData_ThrowsArgumentException()
    {
        var svc = new AesApiKeyCryptoService("testkey");
        Assert.Throws<ArgumentException>(() => svc.Decrypt(new byte[16]));
    }

    [Fact]
    public void AesCrypto_Decrypt_NullData_ThrowsArgumentException()
    {
        var svc = new AesApiKeyCryptoService("testkey");
        Assert.Throws<ArgumentException>(() => svc.Decrypt(null!));
    }

    [Fact]
    public void AesCrypto_EncryptDecrypt_RoundTrip()
    {
        var svc = new AesApiKeyCryptoService("my-secret-key");
        string original = "Hello, AES!";

        byte[] encrypted = svc.Encrypt(original);
        string decrypted = svc.Decrypt(encrypted);

        Assert.Equal(original, decrypted);
    }

    [Fact]
    public void AesCrypto_EncryptToString_DecryptToString_RoundTrip()
    {
        var svc = new AesApiKeyCryptoService("my-secret-key");
        string original = "Base64 round-trip test";

        string encrypted = svc.EncryptToString(original);
        string decrypted = svc.DecryptToString(encrypted);

        Assert.Equal(original, decrypted);
    }

    [Fact]
    public void AesCrypto_SameInput_ProducesDifferentCipherEachTime()
    {
        // IV가 매번 랜덤이므로 동일 입력이라도 암호문이 달라야 함
        var svc = new AesApiKeyCryptoService("my-secret-key");
        string plain = "same input";

        byte[] enc1 = svc.Encrypt(plain);
        byte[] enc2 = svc.Encrypt(plain);

        Assert.NotEqual(enc1, enc2);
    }

    // =========================================================================
    // AESEncryption
    // =========================================================================

    [Fact]
    public void AesEncryption_Constructor_InvalidKeyLength_ThrowsArgumentException()
    {
        // 10글자 = 10바이트 ASCII → AES 키 길이 불일치
        Assert.Throws<ArgumentException>(() => new AESEncryption("shortkey10"));
    }

    [Fact]
    public void AesEncryption_Constructor_EmptyKey_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new AESEncryption(""));
    }

    [Fact]
    public void AesEncryption_Encrypt_EmptyPlainText_ThrowsArgumentException()
    {
        var enc = new AESEncryption("1234567890123456"); // 16바이트
        Assert.Throws<ArgumentException>(() => enc.Encrypt(""));
    }

    [Fact]
    public void AesEncryption_Decrypt_TooShortData_ThrowsArgumentException()
    {
        var enc = new AESEncryption("1234567890123456");
        Assert.Throws<ArgumentException>(() => enc.Decrypt(new byte[8]));
    }

    [Fact]
    public void AesEncryption_EncryptDecrypt_16ByteKey_RoundTrip()
    {
        var enc = new AESEncryption("1234567890123456"); // 16바이트 = AES-128
        string original = "AES-128 test";

        byte[] encrypted = enc.Encrypt(original);
        string decrypted = enc.Decrypt(encrypted);

        Assert.Equal(original, decrypted);
    }

    [Fact]
    public void AesEncryption_EncryptDecrypt_32ByteKey_RoundTrip()
    {
        var enc = new AESEncryption("12345678901234567890123456789012"); // 32바이트 = AES-256
        string original = "AES-256 test";

        byte[] encrypted = enc.Encrypt(original);
        string decrypted = enc.Decrypt(encrypted);

        Assert.Equal(original, decrypted);
    }

    // =========================================================================
    // RSACrypto - XML 키 포맷
    // =========================================================================

    [Fact]
    public void RSA_Xml_EncryptDecrypt_RoundTrip()
    {
        var rsa = new RSACrypto();
        using RSACryptoServiceProvider provider = new RSACryptoServiceProvider(2048);
        string pubKey = provider.ToXmlString(false);
        string privKey = provider.ToXmlString(true);
        string original = "RSA XML round-trip";

        string encrypted = rsa.RSAEncrypt(original, pubKey);
        string decrypted = rsa.RSADecrypt(encrypted, privKey);

        Assert.Equal(original, decrypted);
    }

    [Fact]
    public void RSA_Xml_Encrypt_EmptyValue_ThrowsArgumentException()
    {
        var rsa = new RSACrypto();
        using RSACryptoServiceProvider provider = new RSACryptoServiceProvider(1024);
        string pubKey = provider.ToXmlString(false);

        Assert.Throws<ArgumentException>(() => rsa.RSAEncrypt("", pubKey));
    }

    [Fact]
    public void RSA_Xml_Encrypt_EmptyKey_ThrowsArgumentException()
    {
        var rsa = new RSACrypto();
        Assert.Throws<ArgumentException>(() => rsa.RSAEncrypt("value", ""));
    }

    // =========================================================================
    // RSACrypto - PEM 키 포맷
    // =========================================================================

    [Fact]
    public void RSA_Pem_GenerateKeyPair_ReturnsNonEmptyPems()
    {
        (string pub, string priv) = RSACrypto.GeneratePemKeyPair(2048);

        Assert.Contains("BEGIN PUBLIC KEY", pub);
        Assert.Contains("BEGIN PRIVATE KEY", priv);
    }

    [Fact]
    public void RSA_Pem_EncryptDecrypt_RoundTrip()
    {
        var rsa = new RSACrypto();
        (string pub, string priv) = RSACrypto.GeneratePemKeyPair(2048);
        string original = "RSA PEM round-trip";

        string encrypted = rsa.RSAEncryptWithPem(original, pub);
        string decrypted = rsa.RSADecryptWithPem(encrypted, priv);

        Assert.Equal(original, decrypted);
    }

    [Fact]
    public void RSA_Pem_Encrypt_EmptyValue_ThrowsArgumentException()
    {
        var rsa = new RSACrypto();
        (string pub, _) = RSACrypto.GeneratePemKeyPair(2048);

        Assert.Throws<ArgumentException>(() => rsa.RSAEncryptWithPem("", pub));
    }

    // =========================================================================
    // MD5Crypt
    // =========================================================================

    [Fact]
    public void MD5_CreateMD5_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => MD5Crypt.CreateMD5(null!));
    }

    [Fact]
    public void MD5_CreateMD5_SameInput_ReturnsSameHash()
    {
        string hash1 = MD5Crypt.CreateMD5("hello");
        string hash2 = MD5Crypt.CreateMD5("hello");

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void MD5_CreateMD5_DifferentInput_ReturnsDifferentHash()
    {
        string hash1 = MD5Crypt.CreateMD5("hello");
        string hash2 = MD5Crypt.CreateMD5("world");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void MD5_CreateMD5_KnownValue_ReturnsExpectedHash()
    {
        // "hello" → MD5 uppercase hex = "5D41402ABC4B2A76B9719D911017C592"
        string hash = MD5Crypt.CreateMD5("hello");

        Assert.Equal("5D41402ABC4B2A76B9719D911017C592", hash);
    }

    [Fact]
    public void MD5_CreateMD5_EmptyString_Returns32CharHash()
    {
        string hash = MD5Crypt.CreateMD5("");

        Assert.Equal(32, hash.Length);
    }
}
