using BuyCafe.Models;
using BuyCafe.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using static BuyCafe.Controllers.CustomerCartController;
using static BuyCafe.Controllers.FohPayController;
using static BuyCafe.Controllers.KitchenStaffController;

namespace BuyCafe.Controllers
{
    public class FohOrderController : ApiController
    {
        DBModel db = new DBModel();


        //FO-1 完成訂單(送餐) 的inputClass
        public class InputOrderCompletion
        {
            public int orderId { get; set; }
        }

        //FO-1 完成訂單(送餐)
        [HttpPost]
        [JwtAuthFilter_E]
        [Route("api/foh/orderCompleted")]
        public IHttpActionResult PostFohOrderCompleted([FromBody] InputOrderCompletion input)
        {
            try
            {
                int orderId = input.orderId;

                // 查詢訂單
                var order = db.Order.FirstOrDefault(o => o.Id == orderId && o.OrderStatus == OrderStatusEnum.待取餐);

                if (order != null)
                {
                    order.OrderStatus = OrderStatusEnum.已完成;
                    db.SaveChanges();

                    var response = new
                    {
                        statusCode = 200,
                        code = 0,
                        message = "完成訂單"
                    };

                    return Ok(response);
                }
                else
                {
                    var response = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "找不到這筆訂單"
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


        //FO-2 編輯訂單 的inputClass
        public class InputOrderItemEdit
        {
            public int orderId { get; set; }  //訂單Id
            public int orderItemId { get; set; } //訂單商品Id
            public int serving { get; set; } //修改份數(int) 如果修改份數為0就會把該品項從訂單中移除
        }

        //FO-2 編輯訂單
        [HttpPost]
        [JwtAuthFilter_E]
        [Route("api/foh/editOrderItem")]
        public IHttpActionResult PostFohEditOrderItem([FromBody] InputOrderItemEdit input)
        {
            try
            {
                //確認資料庫有該訂單品項
                var orderItem = db.OrderItem.FirstOrDefault(oi => oi.Id == input.orderItemId && oi.OrderId == input.orderId);
                if (orderItem == null)
                {
                    var errorResponse = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "找不到該訂單品項",
                    };
                    return Ok(errorResponse);
                }

                var order = db.Order.FirstOrDefault(o => o.Id == input.orderId);

                if (order == null)
                {
                    var errorResponse = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "找不到該訂單"
                    };
                    return Ok(errorResponse);
                }

                ////判斷該訂單是否已結束
                //if (order.OrderStatus != OrderStatusEnum.已完成)
                //{
                //    var errorResponse = new
                //    {
                //        statusCode = 400,
                //        code = -1,
                //        message = "該訂單已完成",
                //    };
                //    return Ok(errorResponse);
                //}

                //判斷是不是點數商品
                if (orderItem.Point > 0)
                {
                    if (input.serving == 0)
                    {
                        db.OrderItem.Remove(orderItem);
                        db.SaveChanges();
                    }
                    else
                    {
                        //如果是增加的話 要判斷點數夠不夠用
                        if (input.serving > orderItem.Quantity)
                        {
                            string phoneNumber = order.CustomerPhone;
                            var member = db.Member.FirstOrDefault(m => m.Phone == phoneNumber);
                            int payPoint = (int)order.Items.Where(oi => oi.Point.HasValue).Sum(oi => oi.Point.Value);
                            int memberPoint = member.Point;
                            if (memberPoint < (payPoint + (input.serving - orderItem.Quantity) * orderItem.Point))
                            {
                                var errorResponse = new
                                {
                                    statusCode = 400,
                                    code = -1,
                                    message = "點數不足",
                                };
                                return Ok(errorResponse);
                            }
                        }

                        orderItem.Quantity = input.serving;
                        db.SaveChanges();
                    }
                }

                //沒問題就修改品項，如果0的話就刪掉
                if (input.serving == 0)
                {
                    db.OrderItem.Remove(orderItem);
                    //訂單內的總金額也要扣掉這個品項的價格
                    order.TotalAmount -= (orderItem.Price * orderItem.Quantity);
                    db.SaveChanges();
                }
                else
                {
                    order.TotalAmount -= (orderItem.Price * orderItem.Quantity);
                    orderItem.Quantity = input.serving;
                    order.TotalAmount += (orderItem.Price * orderItem.Quantity);
                //更新資料庫
                    db.SaveChanges();
                }


                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "修改訂單品項成功",
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
