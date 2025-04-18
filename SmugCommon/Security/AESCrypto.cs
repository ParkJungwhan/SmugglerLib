using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace SmugCommon.Security
{
    public class AESCrypto
    {
        private static byte[] pbyteKey;// = Encoding.ASCII.GetBytes("d@wp3nd2");

        public static void SetAPIKey(string key)
        {
            Debug.Assert(false == string.IsNullOrEmpty(key));
            pbyteKey = Encoding.ASCII.GetBytes(key);
        }

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

        public static string EncryptString(string strKey)
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

        public static string DecryptString(string strKey)
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
    }
}