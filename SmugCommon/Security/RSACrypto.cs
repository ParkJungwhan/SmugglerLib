using System.Security.Cryptography;
using System.Text;

namespace SmugCommon.Security
{
    public class RSACrypto
    {
        // need implementation
        public RSACrypto()
        { }

        /// <summary>
        /// </summary>
        public string RSAEncrypt(string getValue, string pubKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(pubKey);
                // 암호화할 문자열을 UTF8 인코딩 byte[]로 변환
                byte[] inbuf = Encoding.UTF8.GetBytes(getValue);
                // 암호화
                byte[] encbuf = rsa.Encrypt(inbuf, false);
                // 암호화된 문자열을 Base64 인코딩하여 반환
                return Convert.ToBase64String(encbuf);
            }
        }

        //public string RSADecrypt
        /// <summary>
        /// 복호화 합니다
        /// </summary>
        public string RSADecrypt(string getValue, string privKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privKey);
                // Base64 인코딩된 문자열을 디코딩하여 byte[]로 변환
                byte[] inbuf = Convert.FromBase64String(getValue);
                // 복호화
                byte[] decbuf = rsa.Decrypt(inbuf, false);
                // 복호화된 byte[]를 UTF8 문자열로 변환하여 반환
                return Encoding.UTF8.GetString(decbuf);
            }
        }

        //private readonly string password = "key";
        //private readonly string encodedString = "";

        //public RSACrypto(string pw = "password", string publickey = "TestKey")
        //{
        //    password = pw;
        //}

        //public string RSAEncrypt(string getValue, string pubKey)
        //{
        //    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        //    rsa.FromXmlString(pubKey);

        // //암호화할 문자열을 UFT8인코딩 byte[] inbuf = (new UTF8Encoding()).GetBytes(getValue);

        // //암호화 byte[] encbuf = rsa.Encrypt(inbuf, false);

        //    //암호화된 문자열 Base64인코딩
        //    return System.Convert.ToBase64String(encbuf);
        //}
    }
}