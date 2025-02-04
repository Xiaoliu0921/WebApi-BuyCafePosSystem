using BuyCafe.Models;
using BuyCafe.Services;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace BuyCafe.Controllers
{
    public class CustomerPayController : ApiController
    {
        DBModel db = new DBModel();

        public class InputOrderCash
        {
            public int orderId { get; set; }
            public string guid { get; set; }
            public int invoice { get; set; }
            public string invoiceCarrier { get; set; }
        }

        //CP-1 送出訂單(現金)
        [HttpPost]
        [Route("api/customer/confirmOrderCash")]
        public IHttpActionResult PostConfirmOrderCash(InputOrderCash input)
        {
            try
            {
                var order = db.Order.FirstOrDefault(o => o.Id == input.orderId && o.Guid == input.guid);
                //判斷訂單是否存在
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

                //判斷訂單是否已送出過
                if (order.OrderStatus != OrderStatusEnum.點餐中)
                {
                    var errorResponse = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "此訂單已送出過"
                    };
                    return Ok(errorResponse);
                }

                if (order.Type == TypeEnum.內用)
                {
                    order.TypeAndNumber = $"內用{order.Table}桌";
                }
                else
                {
                    //可能有送出後又跑回上一步重新送出的訂單 如果已經有外帶編號就不用重給
                    if (order.TypeAndNumber.IsNullOrWhiteSpace()|| !(order.TypeAndNumber.StartsWith("外帶")))
                    {
                        int takeNumber = db.Order.Count(o => DbFunctions.TruncateTime(o.TakeTime) == DbFunctions.TruncateTime(order.TakeTime) && o.Type != TypeEnum.內用 && o.OrderStatus != OrderStatusEnum.點餐中) + 1;
                        order.TypeAndNumber = "外帶" + takeNumber.ToString("D3");
                    }
                    
                }
                order.OrderStatus = OrderStatusEnum.待結帳;
                db.SaveChanges();

                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "送出訂單成功"
                };

                // 訂單創建成功後，發送通知
                NotifyBehavior.BroadcastOrderUpdate(new
                {
                    message = "newOrder",
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

        //CP-4 結帳完成頁面
        [HttpGet]
        [Route("api/customer/getOrder/{guid}")]
        public IHttpActionResult OrderCompleted(string guid)
        {
            try
            {
                var order = db.Order.FirstOrDefault(o => o.Guid == guid && o.OrderStatus != OrderStatusEnum.點餐中);
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

                if (order.Type == TypeEnum.內用)
                {
                    //處理輸出的時間字串
                    string takeTime = order.TakeTime.Value.ToString("MM/dd HH:mm");
                    if (order.TakeTime.Value.Date == DateTime.Now.Date)
                    {
                        takeTime = "今天 " + takeTime;
                    }
                    else if (order.TakeTime.Value.Date == DateTime.Now.Date.AddDays(1))
                    {
                        takeTime = "明天 " + takeTime;
                    }
                    else if (order.TakeTime.Value.Date == DateTime.Now.Date.AddDays(2))
                    {
                        takeTime = "後天 " + takeTime;
                    }
                    
                    var output = new
                    {
                        numberTitle = "內用桌號",
                        number = order.Table.ToString(),
                        type = "內用",
                        takeTime = takeTime,
                        orderStatus = order.OrderStatus == OrderStatusEnum.準備中 ? "已結帳" : order.OrderStatus.ToString(),
                        products = order.Items.Select(oi => new
                        {
                            name = oi.Customization.IsNullOrWhiteSpace()?oi.Name:oi.Name+"("+oi.Customization+")",
                            //customization = oi.Customization.Split(',').ToList(),
                            serving = oi.Quantity,
                            price = oi.Price
                        }),
                        count=order.Items.Sum(oi=>oi.Quantity),
                        totalAmount = order.TotalAmount,
                        note=order.Note
                    };

                    var response = new
                    {
                        statusCode = 200,
                        code = 0,
                        message = "查詢訂單完成畫面成功",
                        data=output
                    };

                    return Ok(response);

                }
                else
                {
                    //處理輸出的時間字串
                    string takeTime = order.TakeTime.Value.ToString("MM/dd HH:mm");
                    if (order.TakeTime.Value.Date == DateTime.Now.Date)
                    {
                        takeTime = "今天 " + takeTime;
                    }
                    else if (order.TakeTime.Value.Date == DateTime.Now.Date.AddDays(1))
                    {
                        takeTime = "明天 " + takeTime;
                    }
                    else if (order.TakeTime.Value.Date == DateTime.Now.Date.AddDays(2))
                    {
                        takeTime = "後天 " + takeTime;
                    }

                    var output = new
                    {
                        numberTitle = "取餐編號",
                        number = order.TypeAndNumber.Replace("外帶",""),
                        type = order.Type.ToString(),
                        takeTime = takeTime,
                        orderStatus = order.OrderStatus == OrderStatusEnum.準備中 ? "已結帳" : order.OrderStatus.ToString(),
                        products = order.Items.Select(oi => new
                        {
                            name = oi.Customization.IsNullOrWhiteSpace() ? oi.Name : oi.Name + "(" + oi.Customization + ")",
                            //customization = oi.Customization.Split(',').ToList(),
                            serving = oi.Quantity,
                            price = oi.Price
                        }).ToList(),
                        count=order.Items.Sum(oi => oi.Quantity),
                        totalAmount = order.TotalAmount,
                        note = order.Note
                    };
                    var response = new
                    {
                        statusCode = 200,
                        code = 0,
                        message = "查詢訂單完成畫面成功",
                        data = output
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
                    data = ex.Message // 返回異常訊息
                };

                return Ok(response);
            }

        }
    }
}
