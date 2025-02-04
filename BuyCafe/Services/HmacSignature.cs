using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace BuyCafe.Services
{
    // 生成Header內所需的"X-LINE-Authorization"
    public class HmacSignature
    {
        public static string GenerateHmacSignature(string key, string url, string requestBody, string nonce)
        {
            // Get方法 : Channel Secret + URI + Query String + nonce
            // Post方法 : Channel Secret + URI + Request Body + nonce
            string message = $"{key}{url}{requestBody}{nonce}";

            // 轉換為bytes array
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            // 透過HMAC算法計算key與message並回傳
            using (var hmac256 = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}