using Smuggler.Common.Security;

namespace TestConsole;

public static class SecurityExample
{
    public static void Run()
    {
        AesApiKeyCryptoService aes = new("platform-secret");
        string aesCipher = aes.EncryptToString("secure-message");
        string aesPlain = aes.DecryptToString(aesCipher);

        RSACrypto rsa = new();
        (string publicKeyPem, string privateKeyPem) = rsa.CreatePemKeyPair();
        string rsaCipher = rsa.RSAEncrypt("rsa-message", publicKeyPem);
        string rsaPlain = rsa.RSADecrypt(rsaCipher, privateKeyPem);

        string checksum = MD5Crypt.CreateChecksum("asset-bundle-v1");

        Console.WriteLine($"AES round-trip: {aesPlain}");
        Console.WriteLine($"RSA round-trip: {rsaPlain}");
        Console.WriteLine($"MD5 checksum: {checksum}");
    }
}
