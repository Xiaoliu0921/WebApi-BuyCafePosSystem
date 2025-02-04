using Jose;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;


namespace BuyCafe.Security
{
    /// <summary>
    /// JwtAuthFilter_C 繼承 ActionFilterAttribute 可生成 [JwtAuthFilter-C] 使用
    /// </summary>
    public class JwtAuthFilter_C : ActionFilterAttribute
    {
        // 加解密的 key，如果不一樣會無法成功解密
        private static readonly string secretKey = WebConfigurationManager.AppSettings["TokenKey"];

        /// <summary>
        /// 過濾有用標籤 [JwtAuthFilter_C] 請求的 API 的 JwtToken 狀態及內容
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // 取出請求內容並排除不需要驗證的 API
            var request = actionContext.Request;
            // 有取到 JwtToken 後，判斷授權格式不存在且不正確時
            if (request.Headers.Authorization == null || request.Headers.Authorization.Scheme != "Bearer")
            {
                SetUnauthenticated(actionContext, "JwtToken Lost");  
            }
            else
            {
                try
                {
                    // 有 JwtToken 且授權格式正確時執行，用 try 包住，因為如果有篡改可能解密失敗
                    // 解密後會回傳 Json 格式的物件 (即加密前的資料)
                    var jwtObject = GetToken(request.Headers.Authorization.Parameter);

                    // 檢查有效期限是否過期，如 JwtToken 過期
                    if (IsTokenExpired(jwtObject["Exp"].ToString()))
                    {
                        //過期
                        SetUnauthenticated(actionContext, "JwtToken Expired");
                    }
                    else
                    {
                        //沒問題就設置IsAuthenticated為true;
                        HttpContext.Current.Items["IsAuthenticated"] = true;
                        HttpContext.Current.Items["PhoneNumber"] = jwtObject["PhoneNumber"];
                    }
                }
                catch (Exception)
                {
                    // 解密失敗
                    SetUnauthenticated(actionContext, "JwtToken NotMatch");
                }
            }
            base.OnActionExecuting(actionContext);
        }

        /// <summary>
        /// 將 Token 解密取得夾帶的資料
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetToken(string token)
        {
            return JWT.Decode<Dictionary<string, object>>(token, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
        }


        private void SetUnauthenticated(HttpActionContext actionContext, string reason = "JwtToken Lost")
        {
            // 設置為未經身份驗證
            HttpContext.Current.Items["IsAuthenticated"] = false;
            HttpContext.Current.Items["AuthReason"] = reason;
        }


        /// <summary>
        /// 驗證 token 時效
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public bool IsTokenExpired(string dateTime)
        {
            return Convert.ToDateTime(dateTime) < DateTime.Now;
        }
    }
}