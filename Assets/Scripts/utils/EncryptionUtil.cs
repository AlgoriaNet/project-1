using System;
using System.Security.Cryptography;
using System.Text;

public class EncryptionUtil
{
    // 使用 PEM 公钥加密数据
    public static string Encrypt(string data, string requestID = "")
    {
        using (RSA rsa = RSA.Create())
        {
            // 从 PEM 格式加载公钥
            rsa.FromXmlString(Config.publicKey);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data + requestID);
            byte[] encryptedBytes = rsa.Encrypt(dataBytes, RSAEncryptionPadding.Pkcs1); // false 表示使用 PKCS#1 填充模式
            return Convert.ToBase64String(encryptedBytes);
        }
    }
}