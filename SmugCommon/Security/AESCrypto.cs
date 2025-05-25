using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace SmugCommon.Security
{
    public class AESCrypto
    {
        // = Encoding.ASCII.GetBytes("d@wp3nd2");
        private readonly byte[] pbyteKey;

        public AESCrypto(string privatekey)
        {
            Debug.Assert(false == string.IsNullOrEmpty(privatekey));
            pbyteKey = Encoding.ASCII.GetBytes(privatekey);
            Debug.Assert(null != pbyteKey, "API 키가 설정되지 않았습니다. SetAPIKey 메서드를 호출하여 키를 설정하세요.");
        }

        [Obsolete("닷넷6.0 이상부터는 이거쓰지말라고 경고함")]
        public byte[] EncryptStringToBytes_AES(string plainText, byte[] key, byte[] IV)
        {
            if (plainText == null || plainText.Length <= 0)
            {
                throw new ArgumentNullException("plainText ");
            }

            if (key == null || key.Length == 0)
            {
                throw new ArgumentNullException("key");
            }

            if (IV == null || IV.Length == 0)
            {
                throw new ArgumentNullException("IV");
            }

            MemoryStream memoryStream = null;
            CryptoStream cryptoStream = null;
            StreamWriter streamWriter = null;
            RijndaelManaged rijndaelManaged = null;
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

        [Obsolete("닷넷6.0 이상부터는 이거쓰지말라고 경고함")]
        public string DecryptBytesToString_AES(byte[] cipherText, byte[] key, byte[] IV)
        {
            if (cipherText == null || cipherText.Length == 0)
            {
                throw new ArgumentNullException("cipherText ");
            }

            if (key == null || key.Length == 0)
            {
                throw new ArgumentNullException("key");
            }

            if (IV == null || IV.Length == 0)
            {
                throw new ArgumentNullException("IV");
            }

            MemoryStream memoryStream = null;
            CryptoStream cryptoStream = null;
            StreamReader streamReader = null;
            RijndaelManaged rijndaelManaged = null;
            string empty = string.Empty;
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

        [Obsolete("닷넷6.0 이상부터는 이거쓰지말라고 경고함")]
        public string EncryptString(string strKey)
        {
            Debug.Assert(null != pbyteKey);

            DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
            dESCryptoServiceProvider.Mode = CipherMode.ECB;
            dESCryptoServiceProvider.Padding = PaddingMode.PKCS7;
            dESCryptoServiceProvider.Key = pbyteKey;
            dESCryptoServiceProvider.IV = pbyteKey;
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, dESCryptoServiceProvider.CreateEncryptor(), CryptoStreamMode.Write);
            byte[] bytes = Encoding.UTF8.GetBytes(strKey.ToCharArray());
            cryptoStream.Write(bytes, 0, bytes.Length);
            cryptoStream.FlushFinalBlock();
            string result = Convert.ToBase64String(memoryStream.ToArray());
            dESCryptoServiceProvider = null;
            return result;
        }

        [Obsolete("닷넷6.0 이상부터는 이거쓰지말라고 경고함")]
        public string DecryptString(string strKey)
        {
            Debug.Assert(null != pbyteKey);

            DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
            dESCryptoServiceProvider.Mode = CipherMode.ECB;
            dESCryptoServiceProvider.Padding = PaddingMode.PKCS7;
            dESCryptoServiceProvider.Key = pbyteKey;
            dESCryptoServiceProvider.IV = pbyteKey;
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, dESCryptoServiceProvider.CreateDecryptor(), CryptoStreamMode.Write);
            strKey = strKey.Replace(" ", "+");
            byte[] array = Convert.FromBase64String(strKey);
            cryptoStream.Write(array, 0, array.Length);
            cryptoStream.FlushFinalBlock();
            string @string = Encoding.UTF8.GetString(memoryStream.GetBuffer());
            memoryStream = null;
            dESCryptoServiceProvider = null;
            return @string.Replace("\0", "");
        }

        public byte[] Encrypt(string plainText)
        {
            Debug.Assert(null != pbyteKey);
            Debug.Assert(false == string.IsNullOrEmpty(plainText));
            Debug.Assert(pbyteKey.Length == 16 || pbyteKey.Length == 24 || pbyteKey.Length == 32, "AES 키 길이는 16, 24, 또는 32 바이트여야 합니다.");

            using Aes aes = Aes.Create();
            aes.Key = pbyteKey;
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
            aes.Key = pbyteKey;
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