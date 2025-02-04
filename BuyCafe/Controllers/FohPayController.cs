using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using BuyCafe.Models;
using Microsoft.Ajax.Utilities;
using BuyCafe.Security;
using System.Web;

namespace BuyCafe.Controllers
{
    public class FohPayController : ApiController
    {
        DBModel db = new DBModel();

        //FP-1的Input Class
        public class InputFohCheckout
        {
            public int orderId {  get; set; }
            public int cash {  get; set; }
            public string note {  get; set; }
            public string invoice { get; set; } = "紙本"; //發票類型 1"載具" 2"統編" 3"捐贈發票" 4"紙本"
            public string invoiceCarrier { get; set; }
            public string phone {  get; set; }
        }


        //FP-1 結帳(外場結帳僅供現金)
        [HttpPost]
        [JwtAuthFilter_E]
        [Route("api/foh/checkout")]
        public IHttpActionResult PostFohCheckout(InputFohCheckout input)
        {
            try
            {
                //檢查訂單是否存在
                //先檢查訂單
                var order = db.Order.FirstOrDefault(o => o.Id == input.orderId);
                if (order == null)
                {
                    var errorResponse = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "找不到該訂單",
                    };
                    return Ok(errorResponse);
                }


                //檢查付款金額有沒有問題
                if (order.TotalAmount > input.cash)
                {
                    var errorResponse = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "付款金額低於訂單金額",
                    };
                    return Ok(errorResponse);
                }

                //再來檢查輸入的手機，如果手機為空就不做檢查
                if (!input.phone.IsNullOrWhiteSpace())
                {
                    //非空的話核對是否是電話格式
                    string pattern = @"^09\d{8}$";
                    Regex regex = new Regex(pattern);
                    if (!regex.IsMatch(input.phone))
                    {
                        var errorResponse = new
                        {
                            statusCode = 400,
                            code = -1,
                            message = "電話號碼格式錯誤",
                        };
                        return Ok(errorResponse);
                    }
                    order.CustomerPhone = input.phone;  //沒問題就存入訂單資訊

                    //能到這邊的就代表前面資訊都正確，該訂單結帳完成了
                    //檢查該會員是否已註冊過，有的話就幫他加上點數，沒的話就繼續
                    var member = db.Member.FirstOrDefault(m => m.Phone == input.phone);
                    if (member != null)
                    {
                        member.Point += (int)order.TotalAmount;
                    }
                }

                //把訂單改成"已結帳"
                order.OrderStatus = OrderStatusEnum.準備中;

                //訂單的備註要記錄
                order.Note = input.note;

                //訂單的發票資訊
                if (Enum.TryParse(input.invoice, out InvoiceEnum result))
                {
                    order.Invoice = result;
                    order.invoiceCarrier = input.invoiceCarrier;
                }
                else
                {
                    //發票形式有錯就不管了XD
                }

                //回傳給前端要有發票號碼，直接用orderId填滿
                string invoiceNumber = order.Id.ToString().PadLeft(8, '0');
                invoiceNumber = "VP-" + invoiceNumber;

                //更新資料庫
                int payPoint = (int)order.Items.Where(oi => oi.Point.HasValue).Sum(oi => oi.Point.Value);
                if (payPoint > 0)
                {
                    string phoneNumber = HttpContext.Current.Items["PhoneNumber"].ToString();
                    var member = db.Member.FirstOrDefault(m => m.Phone == phoneNumber);
                    member.Point -= payPoint;
                    db.SaveChanges();
                }
                else
                {
                    db.SaveChanges();
                }

                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "結帳成功",
                    data = new
                    {
                        invoiceNumber = invoiceNumber,
                        phone = input.phone,
                        cash = input.cash,
                        change = (input.cash - order.TotalAmount),
                        point = order.TotalAmount
                    }
                };

                // 訂單完成結帳後，發送通知給櫃台和廚房
                NotifyBehavior.BroadcastOrderUpdate(new
                {
                    message = "newOrderCheckouted",
                    orderId = order.Id
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    statusCode = 400,
                    code = -1,
                    message = "其他異常",
                    data = ex.Message // 返回異常訊息
                };

                return Ok(response);
            }

        }

    }
}
