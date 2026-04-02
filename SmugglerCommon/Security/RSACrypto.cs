using System.Security.Cryptography;
using System.Text;

namespace Smuggler.Common.Security;

/// <summary>
/// RSA 암복호화 유틸리티.<br/>
/// XML 키 포맷(<see cref="RSAEncrypt"/>, <see cref="RSADecrypt"/>)과
/// PEM 키 포맷(<see cref="RSAEncryptWithPem"/>, <see cref="RSADecryptWithPem"/>)을 모두 지원한다.
/// </summary>
/// <remarks>
/// <b>키 포맷 비교:</b><br/>
/// - XML 포맷: .NET 전통 방식. <c>RSA.ToXmlString()</c>으로 생성. PKCS#1 v1.5 패딩 사용.<br/>
/// - PEM 포맷: 표준 포맷(PKCS#8/SPKI). OpenSSL 등 타 언어와 호환. OAEP-SHA256 패딩 사용(권장).<br/>
/// <br/>
/// <b>패딩 보안 참고:</b><br/>
/// XML 메서드는 PKCS#1 v1.5 패딩을 사용한다. 보안상 OAEP 패딩이 권장되므로
/// 새 코드에서는 PEM 메서드 사용을 권장한다.
/// </remarks>
public class RSACrypto
{
    public RSACrypto() { }

    // -------------------------------------------------------------------------
    // XML 키 포맷 (기존 방식)
    // -------------------------------------------------------------------------

    /// <summary>
    /// XML 공개키로 getValue를 RSA 암호화하고 Base64 문자열을 반환한다.
    /// </summary>
    /// <exception cref="ArgumentException">getValue 또는 pubKey가 null이거나 비어 있을 때.</exception>
    public string RSAEncrypt(string getValue, string pubKey)
    {
        if (string.IsNullOrEmpty(getValue))
            throw new ArgumentException("암호화할 값이 비어 있습니다.", nameof(getValue));
        if (string.IsNullOrEmpty(pubKey))
            throw new ArgumentException("공개키가 비어 있습니다.", nameof(pubKey));

        using RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        rsa.FromXmlString(pubKey);
        byte[] inbuf = Encoding.UTF8.GetBytes(getValue);
        byte[] encbuf = rsa.Encrypt(inbuf, false); // PKCS#1 v1.5
        return Convert.ToBase64String(encbuf);
    }

    /// <summary>
    /// XML 개인키로 getValue를 RSA 복호화하고 원문을 반환한다.
    /// </summary>
    /// <exception cref="ArgumentException">getValue 또는 privKey가 null이거나 비어 있을 때.</exception>
    public string RSADecrypt(string getValue, string privKey)
    {
        if (string.IsNullOrEmpty(getValue))
            throw new ArgumentException("복호화할 값이 비어 있습니다.", nameof(getValue));
        if (string.IsNullOrEmpty(privKey))
            throw new ArgumentException("개인키가 비어 있습니다.", nameof(privKey));

        using RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        rsa.FromXmlString(privKey);
        byte[] inbuf = Convert.FromBase64String(getValue);
        byte[] decbuf = rsa.Decrypt(inbuf, false); // PKCS#1 v1.5
        return Encoding.UTF8.GetString(decbuf);
    }

    // -------------------------------------------------------------------------
    // PEM 키 포맷 (현대적 방식, .NET 5+)
    // -------------------------------------------------------------------------

    /// <summary>
    /// PEM 공개키(SPKI 포맷)로 getValue를 RSA-OAEP-SHA256 암호화하고 Base64 문자열을 반환한다.
    /// </summary>
    /// <param name="pemPublicKey">-----BEGIN PUBLIC KEY----- 형식의 PEM 문자열.</param>
    /// <exception cref="ArgumentException">getValue 또는 pemPublicKey가 null이거나 비어 있을 때.</exception>
    public string RSAEncryptWithPem(string getValue, string pemPublicKey)
    {
        if (string.IsNullOrEmpty(getValue))
            throw new ArgumentException("암호화할 값이 비어 있습니다.", nameof(getValue));
        if (string.IsNullOrEmpty(pemPublicKey))
            throw new ArgumentException("PEM 공개키가 비어 있습니다.", nameof(pemPublicKey));

        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(pemPublicKey);
        byte[] inbuf = Encoding.UTF8.GetBytes(getValue);
        byte[] encbuf = rsa.Encrypt(inbuf, RSAEncryptionPadding.OaepSHA256);
        return Convert.ToBase64String(encbuf);
    }

    /// <summary>
    /// PEM 개인키(PKCS#8 포맷)로 getValue를 RSA-OAEP-SHA256 복호화하고 원문을 반환한다.
    /// </summary>
    /// <param name="pemPrivateKey">-----BEGIN PRIVATE KEY----- 형식의 PEM 문자열.</param>
    /// <exception cref="ArgumentException">getValue 또는 pemPrivateKey가 null이거나 비어 있을 때.</exception>
    public string RSADecryptWithPem(string getValue, string pemPrivateKey)
    {
        if (string.IsNullOrEmpty(getValue))
            throw new ArgumentException("복호화할 값이 비어 있습니다.", nameof(getValue));
        if (string.IsNullOrEmpty(pemPrivateKey))
            throw new ArgumentException("PEM 개인키가 비어 있습니다.", nameof(pemPrivateKey));

        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(pemPrivateKey);
        byte[] inbuf = Convert.FromBase64String(getValue);
        byte[] decbuf = rsa.Decrypt(inbuf, RSAEncryptionPadding.OaepSHA256);
        return Encoding.UTF8.GetString(decbuf);
    }

    /// <summary>
    /// RSA 키 쌍(공개키, 개인키)을 PEM 포맷으로 생성하여 반환한다.
    /// </summary>
    /// <param name="keySize">키 크기(비트). 2048 이상 권장.</param>
    /// <returns>(publicKeyPem, privateKeyPem) 튜플.</returns>
    public static (string PublicKey, string PrivateKey) GeneratePemKeyPair(int keySize = 2048)
    {
        using RSA rsa = RSA.Create(keySize);
        string publicKey = rsa.ExportSubjectPublicKeyInfoPem();
        string privateKey = rsa.ExportPkcs8PrivateKeyPem();
        return (publicKey, privateKey);
    }
}
