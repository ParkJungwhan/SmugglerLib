using System.Security.Cryptography;
using System.Text;

namespace Smuggler.Common.Security;

public static class MD5Crypt
{
    // MD5 is kept only for non-security checksum compatibility.
    public static string CreateChecksum(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        using MD5 md5 = MD5.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        StringBuilder builder = new();
        foreach (byte hashByte in hashBytes)
        {
            builder.Append(hashByte.ToString("X2"));
        }

        return builder.ToString();
    }

    [Obsolete("Use CreateChecksum. MD5 is only appropriate for compatibility checksums, not security-sensitive hashing.")]
    public static string CreateMD5(string input)
    {
        return CreateChecksum(input);
    }
}
