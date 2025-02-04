using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security;
using BuyCafe.Security;
using System.Web;
using System.Data.Entity;
using BuyCafe.Models;
using System.Web.Http.Results;
using System.Diagnostics;
using System.Drawing;
using System.Data.Entity.Validation;

namespace BuyCafe.Controllers
{
    public class CustomerSearchController : ApiController
    {
        DBModel db = new DBModel();

        public class CustomerInf
        {
            public int? tableNumber { get; set; }
            public string phoneNumber { get; set; }
        }

        // CS-1 客戶查詢訂單
        [HttpPost]
        [Route("api/customer/searchOrders")]
        public IHttpActionResult SearchOrders([FromBody] CustomerInf input)
        {
            try
            {
                // 檢查桌號與手機不能同時為空
                if (input.tableNumber == null && input.phoneNumber == null)
                {
                    var errorResponse = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "必須輸入桌號或手機才可以查詢"
                    };

                    return Ok(errorResponse);
                }

                IQueryable<Order> orderList;

                // 桌號與手機號碼皆有輸入，優先以手機號碼查詢
                if (input.phoneNumber != null)
                {
                    orderList = db.Order.Where(o => o.CustomerPhone == input.phoneNumber);
                }
                else // 只有輸入桌號
                {
                    orderList = db.Order.Where(o => o.Table == input.tableNumber);
                }

                if (!orderList.Any())
                {
                    var errorResponse = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = input.phoneNumber != null ? "查無該手機號碼訂單" : "查無該桌號訂單"
                    };

                    return Ok(errorResponse);
                }

                var preparationOrders = orderList.Where(o => o.OrderStatus != OrderStatusEnum.點餐中 && o.OrderStatus != OrderStatusEnum.已完成).ToList();
                var completedOrders = orderList.Where(o => o.OrderStatus == OrderStatusEnum.已完成).ToList();

                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = input.phoneNumber != null ? "回傳手機查詢訂單成功" : "回傳桌號查詢訂單成功",
                    data = new
                    {
                        preparationOrders = preparationOrders.Select(po => new
                        {
                            orderStatus = po.OrderStatus.ToString(),
                            orderType = po.Type.ToString(),
                            orderPayment = po.OrderStatus == OrderStatusEnum.待結帳 ? "待結帳" : "已結帳",
                            orderNumber = po.Id.ToString("D7"),
                            orderAmount = po.TotalAmount,
                            orderItem = po.Items.Select(item => new
                            {
                                name = item.Name,
                                customization = item.Customization,
                                quantity = item.Quantity,
                                price = item.Price,
                                point = item.Point
                            })
                        }).ToList(),

                        completedOrders = completedOrders.Select(co => new
                        {
                            orderStatus = (co.TakeTime.HasValue ? co.TakeTime.Value.ToString("yyyy/MM/dd") : co.CreateDate.ToString("yyyy/MM/dd")) + "已完成",
                            orderType = co.Type.ToString(),
                            orderPayment = "已結帳",
                            orderNumber = co.Id.ToString("D7"),
                            orderAmount = co.TotalAmount,
                            orderItem = co.Items.Select(item => new
                            {
                                name = item.Name,
                                customization = item.Customization,
                                quantity = item.Quantity,
                                price = item.Price,
                                point = item.Point
                            })
                        }).ToList()
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
                    data = ex.Message
                };

                return Ok(response);
            }
        }


        //CS-2 再點一次
        [HttpGet]
        [Route("api/customer/orderAgain/{orderId}")]
        public IHttpActionResult getOrderAgain(string orderId)
        {
            try
            {
                // 將 orderId 轉換為整數，並檢查轉換是否成功
                if (!int.TryParse(orderId, out int orderIdInt))
                {
                    return Ok(new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "無效的訂單 ID"
                    });
                }

                var oldOrder = db.Order.FirstOrDefault(o => o.Id == orderIdInt && o.OrderStatus != OrderStatusEnum.點餐中);
                if (oldOrder != null)
                {
                    // 創建 Guid 並創建一個新的訂單（配上此 Guid）
                    string newGuid = Guid.NewGuid().ToString();
                    Order newOrder = new Order { Guid = newGuid, TotalAmount = oldOrder.TotalAmount };
                    db.Order.Add(newOrder);

                    // 獲取舊訂單項目
                    var oldOrderItems = oldOrder.Items.ToList();

                    foreach (var item in oldOrderItems)
                    {
                        // 創建新的訂單項目
                        OrderItem newItem = new OrderItem
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Name = item.Name,
                            Customization= item.Customization,
                            Price= item.Price,
                            ImagePath= item.ImagePath,
                            // 根據需要複製其他屬性
                            OrderId = newOrder.Id // 設定新訂單的 ID
                        };
                        db.OrderItem.Add(newItem);
                    }

                    db.SaveChanges();

                    var response = new
                    {
                        statusCode = 200,
                        code = 0,
                        message = "取得再點一次之訂單Id跟Guid成功",
                        data = new
                        {
                            orderId = newOrder.Id,
                            guid = newGuid
                        }
                    };

                    return Ok(response);
                }
                else
                {
                    var response = new
                    {
                        statusCode = 200,
                        code = 0,
                        message = "找不到指定之訂單資料",
                    };

                    return Ok(response);
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                // 捕獲具體的驗證錯誤
                var errors = dbEx.EntityValidationErrors
                    .SelectMany(validationErrors => validationErrors.ValidationErrors
                    .Select(validationError => validationError.ErrorMessage))
                    .ToList();

                var response = new
                {
                    statusCode = 400,
                    code = -1,
                    message = "驗證失敗",
                    data = errors // 返回具體的錯誤信息
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


    }
}
