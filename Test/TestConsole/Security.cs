using System;
using System.Security.Cryptography;
using Smuggler.Common.Security;

namespace TestConsole;

/// <summary>
/// C0406: AES / RSA / MD5 암복호화 예제
/// </summary>
internal static class Security
{
    internal static void Run()
    {
        Console.WriteLine("=== Security 예제 시작 ===");
        Console.WriteLine();

        RunAesApiKey();
        RunAesEncryption();
        RunRsaXml();
        RunRsaPem();
        RunMd5();

        Console.WriteLine("=== Security 예제 종료 ===");
    }

    // -------------------------------------------------------------------------

    private static void RunAesApiKey()
    {
        Console.WriteLine("--- AesApiKeyCryptoService (AES-256-CBC) ---");

        var svc = new AesApiKeyCryptoService("my-secret-passphrase");
        string original = "안녕하세요, AES 암호화 테스트입니다.";

        // 바이트 배열 암복호화
        byte[] encrypted = svc.Encrypt(original);
        string decrypted = svc.Decrypt(encrypted);
        Console.WriteLine($"  원문:      {original}");
        Console.WriteLine($"  암호화 바이트: {encrypted.Length}bytes");
        Console.WriteLine($"  복호화:    {decrypted}");

        // Base64 문자열 암복호화
        string encStr = svc.EncryptToString(original);
        string decStr = svc.DecryptToString(encStr);
        Console.WriteLine($"  Base64 암호문: {encStr[..32]}...");
        Console.WriteLine($"  Base64 복호화: {decStr}");

        Console.WriteLine();
    }

    private static void RunAesEncryption()
    {
        Console.WriteLine("--- AESEncryption (AES-128 / AES-256) ---");

        // AES-128: 16바이트 키
        var aes128 = new AESEncryption("1234567890123456");
        string msg128 = "AES-128 메시지";
        string dec128 = aes128.Decrypt(aes128.Encrypt(msg128));
        Console.WriteLine($"  AES-128 round-trip: {dec128}");

        // AES-256: 32바이트 키
        var aes256 = new AESEncryption("12345678901234567890123456789012");
        string msg256 = "AES-256 메시지";
        string dec256 = aes256.Decrypt(aes256.Encrypt(msg256));
        Console.WriteLine($"  AES-256 round-trip: {dec256}");

        // 잘못된 키 길이 예외
        try
        {
            _ = new AESEncryption("shortkey");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"  잘못된 키 길이 예외: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static void RunRsaXml()
    {
        Console.WriteLine("--- RSACrypto XML 키 포맷 ---");

        var rsa = new RSACrypto();
        using RSACryptoServiceProvider provider = new RSACryptoServiceProvider(2048);
        string pubKey = provider.ToXmlString(false);
        string privKey = provider.ToXmlString(true);

        string original = "RSA XML 암호화 메시지";
        string encrypted = rsa.RSAEncrypt(original, pubKey);
        string decrypted = rsa.RSADecrypt(encrypted, privKey);
        Console.WriteLine($"  원문:      {original}");
        Console.WriteLine($"  암호문:    {encrypted[..32]}...");
        Console.WriteLine($"  복호화:    {decrypted}");

        Console.WriteLine();
    }

    private static void RunRsaPem()
    {
        Console.WriteLine("--- RSACrypto PEM 키 포맷 (OAEP-SHA256, 권장) ---");

        var rsa = new RSACrypto();
        (string pubPem, string privPem) = RSACrypto.GeneratePemKeyPair(2048);

        Console.WriteLine($"  공개키 앞부분: {pubPem.Split('\n')[0]}");

        string original = "RSA PEM 암호화 메시지";
        string encrypted = rsa.RSAEncryptWithPem(original, pubPem);
        string decrypted = rsa.RSADecryptWithPem(encrypted, privPem);
        Console.WriteLine($"  원문:      {original}");
        Console.WriteLine($"  암호문:    {encrypted[..32]}...");
        Console.WriteLine($"  복호화:    {decrypted}");

        Console.WriteLine();
    }

    private static void RunMd5()
    {
        Console.WriteLine("--- MD5Crypt (체크섬 전용) ---");

        string[] inputs = { "hello", "world", "SmugglerLib" };
        foreach (string input in inputs)
            Console.WriteLine($"  MD5({input}) = {MD5Crypt.CreateMD5(input)}");

        Console.WriteLine("  ※ MD5는 파일 체크섬 등 비보안 목적에만 사용. 보안 해시는 SHA256 사용 권장.");
        Console.WriteLine();
    }
}
