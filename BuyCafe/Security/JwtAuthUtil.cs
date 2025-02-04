using BuyCafe.Models;
using Jose;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Web.Configuration;

namespace BuyCafe.Security
{
    public class JwtAuthUtil
    {
        // 自訂字串，驗證用，用來加密送出的 key (放在 Web.config 的 appSettings)
        private static readonly string secretKey = WebConfigurationManager.AppSettings["TokenKey"];

        /// <summary>
        /// 生成 JwtToken
        /// </summary>
        /// <returns>JwtToken</returns>
        /// 
        public string GenerateToken_Customer(string phoneNumber)
        {
            // payload 需透過 token 傳遞的資料 (可夾帶常用且不重要的資料)
            var payload = new Dictionary<string, object>
            {
                { "PhoneNumber", phoneNumber },
                { "Exp", DateTime.Now.AddHours(12).ToString() } // JwtToken 時效設定 12小時 
            };

            // 產生 JwtToken
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;

        }

        public string GenerateToken_Employee(string account, IdentityEnum identityEnum)
        {
            // payload 需透過 token 傳遞的資料 (可夾帶常用且不重要的資料)
            var payload = new Dictionary<string, object>
            {
                { "Account", account },     //帳號(知道登入者)
                { "Identity", identityEnum},  //登入身分(1外場2內場3店長)
                { "Exp", DateTime.Now.AddHours(12).ToString() } // JwtToken 時效設定 12小時 
            };

            // 產生 JwtToken
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;

        }


        ///// <summary>
        ///// 生成只刷新效期的 JwtToken
        ///// </summary>
        ///// <returns>JwtToken</returns>
        //public string ExpRefreshToken(Dictionary<string, object> tokenData)
        //{
        //    string secretKey = WebConfigurationManager.AppSettings["TokenKey"];
        //    // payload 從原本 token 傳遞的資料沿用，並刷新效期
        //    var payload = new Dictionary<string, object>
        //    {
        //        //{ "Id", (int)tokenData["Id"] },
        //        //{ "Account", tokenData["Account"].ToString() },
        //        //{ "NickName", tokenData["NickName"].ToString() },
        //        //{ "Image", tokenData["Image"].ToString() },
        //        { "Exp", DateTime.Now.AddMinutes(30).ToString() } // JwtToken 時效刷新設定 30 分
        //    };

        //    //產生刷新時效的 JwtToken
        //    var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
        //    return token;
        //}

        /// <summary>
        /// 生成無效 JwtToken
        /// </summary>
        /// <returns>JwtToken</returns>
        public string RevokeToken()
        {
            string secretKey = "RevokeToken"; // 故意用不同的 key 生成
            var payload = new Dictionary<string, object>
            {
                { "PhoneNumber", "None" },
                { "Account", "None" },     //帳號(知道登入者)
                { "Identity", "None"},  //登入身分(1外場2內場3店長)
                { "Exp", DateTime.Now.AddDays(-15).ToString() } // 使 JwtToken 過期 失效
            };

            // 產生失效的 JwtToken
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }
    }
}