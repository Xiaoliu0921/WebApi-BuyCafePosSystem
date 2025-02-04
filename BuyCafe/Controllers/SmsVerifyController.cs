using BuyCafe.Models;
using BuyCafe.Security;
using BuyCafe.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Twilio.Types;

namespace BuyCafe.Controllers
{
    public class SmsVerifyController : ApiController
    {
        DBModel db = new DBModel();
        private readonly SmsService smsService = new SmsService();

        public class CustomerPhoneNumber
        {
            public string phoneNumber { get; set; }
        }

        /*  CL-1 
         * 1. 檢查手機格式
         * 2. 檢查是否存在資料庫，不存在就儲存，並將歷史訂單金額換算點數儲存
         * 3. 發送簡訊
        */
        [HttpPost]
        [Route("api/customer/sendVerifyCode")]
        public async Task<IHttpActionResult> SendVerifyCode([FromBody] CustomerPhoneNumber phone)
        {
            try
            {
                string customerPhone = phone.phoneNumber;

                // 檢查手機格式，false回傳"格式錯誤"
                if (!smsService.CheckPhoneFormat(customerPhone))
                {
                    var response = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "手機格式錯誤"
                    };
                    return Ok(response);
                }
                else
                {
                    var isExist = db.Member.Any(m => m.Phone == customerPhone);

                    // 若顧客手機不存在資料庫，先新增顧客手機後再發送驗證碼
                    if (!isExist)
                    {
                        // 撈取歷史訂單，並更新顧客點數
                        int? historyOrdersAmount = db.Order.Where(o => o.CustomerPhone == customerPhone && o.OrderStatus == OrderStatusEnum.已完成).Sum(o => o.TotalAmount) ?? 0;

                        Member newMember = new Member
                        {
                            Phone = customerPhone,
                            Point = (int) historyOrdersAmount
                        };

                        db.Member.Add(newMember);        
                        db.SaveChanges();

                        await smsService.GenerateCodeAndSendAsync(customerPhone);

                        var response = new
                        {
                            statusCode = 200,
                            code = 0,
                            message = "簡訊驗證碼發送成功"
                        };

                        return Ok(response);
                    }
                    else
                    {
                        await smsService.GenerateCodeAndSendAsync(customerPhone);

                        var response = new
                        {
                            statusCode = 200,
                            code = 0,
                            message = "簡訊驗證碼發送成功"
                        };

                        return Ok(response);
                    }
                }
            }
            catch(Exception ex)
            {
                var response = new
                {
                    statusCode = 400,
                    code = -1,
                    message = "其他異常",
                    data = ex.Message
                };

                return Ok(response);
            }
        }

        public class CustomerVerify
        {
            public string phoneNumber { get; set; }
            public string verifyCode { get; set; }
        }

        // CL-2 簡訊驗證
        [HttpPost]
        [Route("api/customer/smsVerify")]
        public IHttpActionResult VerifyCode([FromBody] CustomerVerify customerVerify)
        {
            try
            { 
                var customerPhone = customerVerify.phoneNumber;
                var verifyCode = customerVerify.verifyCode;

                // 檢查手機格式，false回傳"格式錯誤"
                if (!smsService.CheckPhoneFormat(customerPhone))
                {
                    var response = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "手機格式錯誤"
                    };
                    return Ok(response);
                }

                // 檢查驗證碼是否為空
                if (string.IsNullOrEmpty(verifyCode))
                {
                    var response = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "驗證碼不能為空"
                    };
                    return Ok(response);
                }

                // 基本檢查後開始驗證流程並回傳結果，成功就發送Token
                var result = smsService.VerifyCustomer(customerPhone, verifyCode);

                switch (result)
                {
                    case VerificationResult.Success:

                        // 生成Token並回傳
                        JwtAuthUtil token = new JwtAuthUtil();
                        string customerToken = token.GenerateToken_Customer(customerPhone);

                        var successResponse = new
                        {
                            statusCode = 200,
                            code = 0,
                            message = "登入成功",
                            data = customerToken
                        };
                        return Ok(successResponse);

                    case VerificationResult.InvalidCode:
                        var invalidCodeResponse = new
                        {
                            statusCode = 400,
                            code = -1,
                            message = "驗證碼錯誤"
                        };
                        return Ok(invalidCodeResponse);

                    case VerificationResult.CodeExpired:
                        var codeExpiredResponse = new
                        {
                            statusCode = 400,
                            code = -1,
                            message = "驗證碼已過期"
                        };
                        return Ok(codeExpiredResponse);

                    default:
                        var unknownErrorResponse = new
                        {
                            statusCode = 400,
                            code = -1,
                            message = "未知錯誤"
                        };
                        return Ok(unknownErrorResponse);
                }
            }
            catch (Exception ex) 
            {
                var response = new
                {
                    statusCode = 400,
                    code = -1,
                    message = "其他異常",
                    data = ex.Message
                };

                return Ok(response);
            }
        }

        // CL-3 取得會員點數
        [HttpGet]
        [Route("api/customer/getPoint")]
        [JwtAuthFilter_C]
        public IHttpActionResult GetPoint()
        {
            try
            {
                if ((bool)HttpContext.Current.Items["IsAuthenticated"] == false)
                {
                    var response = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "身份驗證錯誤，請重新登入"
                    };
                    return Ok(response);
                }

                string phoneNumber = HttpContext.Current.Items["PhoneNumber"] as string;
                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    var memberInf = db.Member.FirstOrDefault(m => m.Phone == phoneNumber);

                    var response = new
                    {
                        statusCode = 200,
                        code = 0,
                        message = "點數回傳成功",
                        data = new
                        {
                            phone = memberInf.Phone,
                            point = memberInf.Point
                        }
                    };

                    return Ok(response);
                }
                else
                {
                    var response = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "Token內找不到手機號碼，請重新登入"
                    };

                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    statusCode = 400,
                    code = -1,
                    message = "其他異常",
                    data = ex.Message
                };

                return Ok(response);
            }
        }
    }
}
