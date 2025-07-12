using System.Security.Cryptography;
using System.Text;

namespace SmugCommon.Security
{
    public class AESEncryption
    {
        private readonly byte[] key;

        public AESEncryption(string stringkey)
        {
            this.key = Encoding.ASCII.GetBytes(stringkey);
        }

        public byte[] Encrypt(string plainText)
        {
            using Aes aes = Aes.Create();
            aes.Key = this.key;
            aes.GenerateIV(); // 랜덤 IV 생성
            aes.Mode = CipherMode.CBC;

            using var encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // IV + 암호문 조합으로 반환
            byte[] result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
            return result;
        }

        public string Decrypt(byte[] encryptedData)
        {
            using Aes aes = Aes.Create();
            aes.Key = this.key;
            aes.Mode = CipherMode.CBC;

            byte[] iv = new byte[16];
            byte[] cipherText = new byte[encryptedData.Length - 16];

            Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(encryptedData, iv.Length, cipherText, 0, cipherText.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}