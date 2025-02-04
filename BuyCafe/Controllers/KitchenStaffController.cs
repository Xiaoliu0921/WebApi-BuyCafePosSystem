using BuyCafe.Models;
using BuyCafe.Security;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BuyCafe.Controllers
{
    public class KitchenStaffController : ApiController
    {
        DBModel db = new DBModel();

        // BS-1 內場訂單總覽
        [HttpGet]
        [JwtAuthFilter_E]
        [Route("api/boh/getOrder")]
        public IHttpActionResult DisplayOrder([FromUri] string type = null, [FromUri] string orderBy = null, [FromUri] string search = null)
        {
            try
            {
                // 取出當天所有準備中的訂單以及訂單細項，此時還沒開始執行查詢
                IQueryable<Order> orders = db.Order.Include("Items").Where(o => o.OrderStatus == OrderStatusEnum.準備中 && DbFunctions.TruncateTime(o.TakeTime) == DbFunctions.TruncateTime(DateTime.Today));

                // 當有帶入type參數
                if (type == "內用"||type=="1")
                {
                    orders = orders.Where(o => o.Type == TypeEnum.內用);

                }
                else if (type == "外帶"|| type == "預約自取"||type=="2" || type == "3")
                {
                    orders = orders.Where(o => o.Type == TypeEnum.外帶|| o.Type == TypeEnum.預約自取);
                }

                // 依據時間升、降序
                if(orderBy=="時間越早優先")
                {
                    orders = orders.OrderBy(o => o.TakeTime);
                }
                else if(orderBy == "時間越晚優先")
                {
                    orders = orders.OrderByDescending(o => o.TakeTime);
                }
                else
                {
                    orders = orders.OrderBy(o => o.TakeTime);
                }

                // 搜尋框的判斷
                if (!(string.IsNullOrEmpty(search))||search=="any")
                {
                    orders = orders.Where(o => o.TypeAndNumber.Contains(search) || o.CustomerPhone.Contains(search));
                }

                List<string> additionalItem = new List<string>
                {
                    "奶油烤吐司",
                    "焦糖胡桃海鹽軟餅乾",
                    "巧克力棉花糖軟餅乾",
                    "肉桂捲捲",
                    "經典奶油可頌"
                };

                var orderInf = orders.ToList();

                foreach (var order in orderInf)
                {
                    // 將 ICollection<OrderItem> 轉換為 List<OrderItem> 以便操作
                    List<OrderItem> itemsList = order.Items.ToList();

                    List<OrderItem> additionalItemsToAdd = new List<OrderItem>();

                    // 遍歷 List<OrderItem>
                    for (int i = 0; i < itemsList.Count; i++)
                    {
                        var item = itemsList[i];

                        foreach (var addition in additionalItem)
                        {
                            if (item.Customization.Contains(addition.Trim()))
                            {
                                // 創建新的加價購品項
                                OrderItem newItem = new OrderItem
                                {
                                    Name = "(加價購)"+addition,
                                    ProductId = item.ProductId,
                                    OrderId = item.OrderId,
                                    Quantity = item.Quantity
                                };

                                // 將加價購品項加入臨時清單
                                additionalItemsToAdd.Add(newItem);

                                // 從客製化選項中移除加價購詞語
                                item.Customization = item.Customization.Replace("," + addition, "").Replace(addition, "").Trim();
                            }
                        }
                    }

                    // 最後將新的加價購項目批量加入原集合
                    if (additionalItemsToAdd.Any())
                    {
                        foreach (var newItem in additionalItemsToAdd)
                        {
                            order.Items.Add(newItem); // ICollection 支援添加新項目
                        }
                    }
                }


                var orderInfFormat = orderInf.Select(o => new
                {
                    orderId = o.Id,
                    typeAndNumber = o.TypeAndNumber,
                    time = o.Type == TypeEnum.預約自取 ? $"{o.TakeTime.Value:HH:mm}取餐" : $"{o.TakeTime.Value:HH:mm}點餐",
                    items = o.Items.Select(oi => new
                    {
                        Name=oi.Name,
                        Quantity=oi.Quantity,
                        Customization=oi.Customization.IsNullOrEmpty()?null: oi.Customization
                    }).ToList()
                }).ToList();

                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "訂單回傳成功",
                    data = orderInfFormat
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
                    data = ex.Message
                };

                return Ok(response);
            }
        }

        public class OrderDelete
        {
            public int OrderId { get; set; }
            public List<int> OrderItems { get; set; }
        }

        // BO-1 刪除餐點
        [HttpDelete]
        [JwtAuthFilter_E]
        [Route("api/boh/editOrder")]
        public IHttpActionResult OrderItemsDelete([FromBody] OrderDelete input)
        {
            try
            {
                var orderIsExist = db.Order.Any(o => o.Id == input.OrderId && o.OrderStatus == OrderStatusEnum.準備中);

                if (orderIsExist)
                {
                    var orderItemDelete = db.OrderItem.Where(oi => oi.OrderId == input.OrderId && input.OrderItems.Contains(oi.Id));

                    if (orderItemDelete.Any())
                    {
                        db.OrderItem.RemoveRange(orderItemDelete);
                        db.SaveChanges();

                        var order = db.Order.FirstOrDefault(o => o.Id == input.OrderId);
                        var orderInf = new
                        {
                            orderId = order.Id,
                            typeAndNumber = order.TypeAndNumber,
                            time = order.Type == TypeEnum.預約自取 ? $"{order.TakeTime.Value:HH:mm}取餐" : $"{order.TakeTime.Value:HH:mm}點餐",
                            items = order.Items.Select(oi => new
                            {
                                oi.Name,
                                oi.Quantity,
                                oi.Customization
                            })
                        };

                        var response = new
                        {
                            statusCode = 200,
                            code = 0,
                            message = "刪除品項成功",
                            data = orderInf
                        };
                        return Ok(response);
                    }
                    else
                    {
                        var response = new
                        {
                            statusCode = 400,
                            code = -1,
                            message = "該訂單內未包含要刪除的品項"
                        };
                        return Ok(response);
                    }
                }
                else
                {
                    var response = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "該訂單不存在"
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

        public class OrderCompletionRequest
        {
            public int OrderId { get; set; }
        }

        // BO-2 備餐完成
        [HttpPost]
        [JwtAuthFilter_E]
        [Route("api/boh/orderCompleted")]
        public IHttpActionResult OrderCompleted([FromBody] OrderCompletionRequest orderCompletionRequest)
        {
            try 
            {
                int orderId = orderCompletionRequest.OrderId;
                // 查詢訂單
                var order = db.Order.FirstOrDefault(o => o.Id == orderId && o.OrderStatus == OrderStatusEnum.準備中);

                if (order != null)
                {
                    order.OrderStatus = OrderStatusEnum.待取餐;
                    db.SaveChanges();

                    var response = new
                    {
                        statusCode = 200,
                        code = 0,
                        message = "完成備餐"
                    };

                    // 廚房完成訂單後，發送通知給櫃台和廚房
                    NotifyBehavior.BroadcastOrderUpdate(new
                    {
                        message = "orderCompleted",
                        orderId = orderId
                    });

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

        //BM-1 餐點供應狀況-總覽

    }
}
