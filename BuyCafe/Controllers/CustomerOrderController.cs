using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BuyCafe.Models;
using Newtonsoft.Json;

namespace BuyCafe.Controllers
{
    public class CustomerGuidController : ApiController
    {
        private DBModel db = new DBModel();

        //CO-1 取得OrderId跟Guid(唯一識別碼)
        [HttpGet]
        [Route("api/customer/getOrderId")]
        public IHttpActionResult GetOrderId()
        {
            try
            {
                //創建Guid並創建一個新的訂單(配上此Guid)
                string newGuid = Guid.NewGuid().ToString();
                Order order = new Order { Guid = newGuid ,TotalAmount=0};
                db.Order.Add(order);
                db.SaveChanges();

                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "取得訂單Id跟Guid成功",
                    data = new  {
                                    orderId = order.Id,
                                    guid = newGuid
                                }
            };
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


        //CO-2 取得現在購物車的商品筆數跟總價
        [HttpGet]
        [Route("api/customer/getOrderInfo/{orderId}/{guid}")]
        public IHttpActionResult GetOrderInfo(int orderId,string guid)
        {
            try
            {
                var existingOrder=db.Order.FirstOrDefault(o => o.Id == orderId&&o.Guid==guid);
                if (existingOrder == null)
                {
                    var errorResponse = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "訂單不存在"
                    };
                    return Ok(errorResponse);
                }


                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "取得購物車基本資訊成功",
                    data = new
                    {
                        count = existingOrder.Items.Sum(oi=>oi.Quantity),
                        totalAmount = existingOrder.TotalAmount
                    }
                };
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
