using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GTPTracker.Functions
{
    public  class AES
    {

         private static byte[] key = { 111, 22, 3, 44, 35, 36, 47, 18, 29, 110, 131, 142, 113, 14,
                                            15, 16, 117, 18, 19, 20, 213, 22, 233, 24};
         private static byte[] iv16Bit = { 1, 2, 3, 54, 5, 6, 57, 8, 9, 10, 11, 152, 13, 14, 155, 16 };

         public static string AesEncryption(string dataToEncrypt)
        {
            var bytes = Encoding.Default.GetBytes(dataToEncrypt);
            using (var aes = new AesCryptoServiceProvider())
            {
                using (var ms = new MemoryStream())
                using (var encryptor = aes.CreateEncryptor(key, iv16Bit))
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cs.Write(bytes, 0, bytes.Length);
                    cs.FlushFinalBlock();
                    var cipher = ms.ToArray();
                    return Convert.ToBase64String(cipher);
                }
            }
        }

         public static string AesDecryption(string dataToDecrypt)
         {
             if (dataToDecrypt != null)
             {
                 var bytes = Convert.FromBase64String(dataToDecrypt);
                 using (var aes = new AesCryptoServiceProvider())
                 {
                     using (var ms = new MemoryStream())
                     using (var decryptor = aes.CreateDecryptor(key, iv16Bit))
                     using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                     {
                         cs.Write(bytes, 0, bytes.Length);
                         cs.FlushFinalBlock();
                         var cipher = ms.ToArray();
                         return Encoding.UTF8.GetString(cipher);
                     }
                 }
             }
             else return null;
         }
    }
}