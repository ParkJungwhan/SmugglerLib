using SmugCommon.Security;
using System.Diagnostics;
using System.Text;

namespace TestConsole
{
    public static class Crypt
    {
        public static void CrpytTest()
        {
            string efun = "FE7BD31EC7C3F9152ECF20142DD6BB99";
            int serverid = 20102;
            string platform = "9006962647";
            var md5result = MD5Crypt.CreateMD5($"{platform}{serverid}1232643{efun}");
            //var md5result = MD5Crypt.CreateMD5("FE7BD31EC7C3F9152ECF20142DD6BB99900696264720102");

            return;

            string privatekey = "fowplatformdiv1!";
            string textdata = "hllowpjhdatas";

            Debug.Assert(!string.IsNullOrEmpty(privatekey), "privatekey is null or empty");
            Debug.Assert(privatekey.Length == 16);

            var key = Encoding.ASCII.GetBytes(privatekey);

            AESEncryption aes1 = new AESEncryption(privatekey);
            var returnEncrypt = aes1.Encrypt(textdata);
            Console.WriteLine($"{returnEncrypt}");

            var returnEncrypt2 = aes1.Encrypt(textdata);
            Console.WriteLine($"{returnEncrypt2}");

            string dec1 = aes1.Decrypt(returnEncrypt);
            string dec2 = aes1.Decrypt(returnEncrypt2);

            AESCrypto aes2 = new AESCrypto(privatekey);
            var result2 = aes2.Encrypt(textdata);
            Console.WriteLine($"{result2}");

            var result3 = aes2.Encrypt(textdata);
            Console.WriteLine($"{result3}");

            var res2 = aes2.Decrypt(result2);
            var res3 = aes2.Decrypt(result3);

            var res4 = aes2.Decrypt(returnEncrypt2);
        }
    }
}