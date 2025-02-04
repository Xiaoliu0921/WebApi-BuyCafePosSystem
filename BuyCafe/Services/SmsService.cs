using BuyCafe.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace BuyCafe.Services
{
    public class SmsService
    {
        string sid = ConfigurationManager.AppSettings["sid"];
        string token = ConfigurationManager.AppSettings["token"];
        string phone = ConfigurationManager.AppSettings["phone"];
        readonly MemoryCache cache = MemoryCache.Default;

        // 檢查手機格式
        public bool CheckPhoneFormat(string customerPhone)
        {
            string pattern = @"^09\d{8}$";
            Regex regex = new Regex(pattern);

            if (regex.IsMatch(customerPhone))
            {

                return true;
            }
            else
            {
                return false;
            }
        }

        /*
           1. 將0替換為+886格式
           2. 產生6碼的隨機數字，並儲存在Cache中，Key為手機號碼，Value為驗證碼，過期時間5分鐘
           3. 透過Twilio發送驗證碼
       */
        public async Task GenerateCodeAndSendAsync(string customerPhone)
        {
            string replaceNumber = $"+886{customerPhone.Substring(1)}";

            Random random = new Random();
            string verifyCode = random.Next(100000, 999999).ToString();

            var cacheItemPolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
            };
            cache.Set(customerPhone, verifyCode, cacheItemPolicy);

            TwilioClient.Init(sid, token);

            await MessageResource.CreateAsync
            (
                to: new PhoneNumber(replaceNumber),
                from: new PhoneNumber(phone),
                body: $"【Buy咖通知】{verifyCode}為您的驗證碼，五分鐘內有效，請勿洩漏。"
            );
        }

        // 驗證顧客身份
        public VerificationResult VerifyCustomer(string customerPhone, string verifyCode)
        {
            string cacheCode = cache.Get(customerPhone) as string;

            if (cacheCode != null)
            {
                if (verifyCode == cacheCode)
                {
                    return VerificationResult.Success; // 輸入驗證碼與Cache內的一樣，正確
                }
                else
                {
                    return VerificationResult.InvalidCode; // 輸入驗證碼與Cache內的不同，錯誤
                }
            }
            else
            {
                return VerificationResult.CodeExpired; // Cache中，找不到驗證碼，視為過期
            }
        }
    }
}