using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Results;
using BuyCafe.Models;
using Jose;
using Microsoft.Ajax.Utilities;
using BuyCafe.Security;
using System.Web;

namespace BuyCafe.Controllers
{
    public class CustomerCartController : ApiController
    {
        DBModel db = new DBModel();

        //CC-1 取得購物車現有訂單
        [HttpGet]
        [Route("api/customer/getCart/{orderId}/{guid}")]
        public IHttpActionResult GetOrderId(int orderId, string guid)
        {
            try
            {
                var order = db.Order.FirstOrDefault(o => o.Id == orderId && o.Guid == guid);
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

                //判斷該訂單是否已結束
                if (order.OrderStatus != OrderStatusEnum.點餐中)
                {
                    var errorResponse = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "該訂單已送出過",
                    };
                    return Ok(errorResponse);
                }

                var Items = order.Items.Select(i => new
                {
                    orderItemId = i.Id,
                    name = i.Name,
                    imagePath = i.ImagePath,
                    customization = i.Customization.Split(',').ToList(),
                    serving = i.Quantity,
                    price = i.Price,
                }).ToList();



                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "取得購物車資訊成功",
                    data = Items
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


        //CC-2的inputJson
        public class InputCartEdit
        {
            public int orderId { get; set; }
            public int orderItemId { get; set; }
            public int serving { get; set; }
        }

        //CC-2 購物車訂單編輯(修改份數)
        [HttpPost]
        [JwtAuthFilter_C]
        [Route("api/customer/editCart")]
        public IHttpActionResult PostEditCart([FromBody] InputCartEdit input)
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

                //判斷該訂單是否已結束
                if (order.OrderStatus != OrderStatusEnum.點餐中)
                {
                    var errorResponse = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "該訂單已送出過",
                    };
                    return Ok(errorResponse);
                }

                //判斷是不是點數商品
                if(orderItem.Point>0)
                {
                    if (input.serving == 0)
                    {
                        db.OrderItem.Remove(orderItem);
                        db.SaveChanges();
                    }
                    else
                    {
                        //如果是增加的話 要判斷點數夠不夠用
                        if(input.serving>orderItem.Quantity)
                        {
                            string phoneNumber = HttpContext.Current.Items["PhoneNumber"].ToString();
                            var member = db.Member.FirstOrDefault(m => m.Phone == phoneNumber);
                            int payPoint = (int)order.Items.Where(oi => oi.Point.HasValue).Sum(oi => oi.Point.Value);
                            int memberPoint = member.Point;
                            if (memberPoint < (payPoint + (input.serving-orderItem.Quantity)*orderItem.Point))
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
                    db.SaveChanges();
                }


                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "編輯購物車品項成功",
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

        //CC-3的inputJson
        public class InputCheckout
        {
            public int orderId { get; set; }
            public string guid { get; set; }
            public string phone { get; set; }
            public string type { get; set; }
            public string table { get; set; }
            public string takeDate { get; set; }
            public string takeTime { get; set; }
            public string note { get; set; }
        }


        //CC-3 前往結帳
        [HttpPost]
        [JwtAuthFilter_C]
        [Route("api/customer/goCheckout")]
        public IHttpActionResult PostGoCheckout([FromBody] InputCheckout input)
        {
            try
            {
                //先檢查訂單
                var order = db.Order.FirstOrDefault(o => o.Id == input.orderId && o.Guid == input.guid);
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

                //找到訂單後來確認其他傳入值是否有錯(電話、用餐類型、桌號、時間)
                //沒錯就存入

                //1.電話號碼
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
                    order.CustomerPhone = input.phone;  //沒問題就存入
                }

                //2.用餐類型
                if (Enum.TryParse(input.type, out TypeEnum resultType))
                {
                    order.Type = resultType;
                    if (resultType == TypeEnum.內用)
                    {
                        if (input.table.IsNullOrWhiteSpace())
                        {
                            var errorResponse = new
                            {
                                statusCode = 400,
                                code = -1,
                                message = "內用桌號不可為空",
                            };
                            return Ok(errorResponse);
                        }
                        else
                        {
                            if (!Int32.TryParse(input.table, out int tableNum))
                            {
                                var errorResponse = new
                                {
                                    statusCode = 400,
                                    code = -1,
                                    message = "內用桌號錯誤(非數字)",
                                };
                                return Ok(errorResponse);
                            }
                            else
                            {
                                order.Table = tableNum;
                            }
                        }
                    }
                    else if(resultType==TypeEnum.預約自取)
                    {
                        if (input.takeDate.IsNullOrWhiteSpace())
                        {
                            var errorResponse = new
                            {
                                statusCode = 400,
                                code = -1,
                                message = "預約自取需填寫時間",
                            };
                            return Ok(errorResponse);
                        }
                    }
                }
                else
                {
                    var errorResponse = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "用餐類型錯誤",
                    };
                    return Ok(errorResponse);
                }


                //處理時間字串
                if (input.takeTime.IsNullOrWhiteSpace()||input.takeDate.IsNullOrWhiteSpace())
                {
                    order.TakeTime = DateTime.Now;  //現場外帶跟內用的取餐時間改成now
                }
                else
                {
                    string dateTimeString;
                    if (input.takeDate.Length == 10)
                    {
                        dateTimeString = input.takeDate + " " + input.takeTime;
                    }
                    else
                    {
                        dateTimeString = input.takeDate.Remove(10) + " " + input.takeTime;
                    }
                    if (DateTime.TryParseExact(dateTimeString, "yyyy-MM-dd HH:mm",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime result))
                    {
                        order.TakeTime = result;
                    }
                    else
                    {
                        var errorResponse = new
                        {
                            statusCode = 400,
                            code = -1,
                            message = "無法解析時間字串",
                        };
                        return Ok(errorResponse);
                    }
                }



                //order.TakeTime = string.IsNullOrWhiteSpace(input.takeTime.ToString()) ? null : input.takeTime;

                ////前面都沒問題後，訂單狀態從點餐中->待結帳
                //if (order.OrderStatus == OrderStatusEnum.點餐中)
                //{
                //    order.OrderStatus = OrderStatusEnum.待結帳;
                //}

                //判斷是否有點數兌換商品
                int payPoint = (int)order.Items.Where(oi => oi.Point.HasValue).Sum(oi => oi.Point.Value);
                if (payPoint > 0)
                {
                    if((bool)HttpContext.Current.Items["IsAuthenticated"])
                    {
                            string phoneNumber = HttpContext.Current.Items["PhoneNumber"].ToString();

                            var member = db.Member.FirstOrDefault(m => m.Phone == phoneNumber);
                            if (member != null)
                            {
                                int memberPoint = member.Point;
                                if(memberPoint < payPoint)
                                {
                                    var errorResponse = new
                                    {
                                        statusCode = 400,
                                        code = -1,
                                        message = "顧客點數不足",
                                    };
                                    return Ok(errorResponse);
                                }
                            }
                            else
                            {
                                var errorResponse = new
                                {
                                    statusCode = 400,
                                    code = -1,
                                    message = "登入帳號異常，請重新登入",
                                };
                                return Ok(errorResponse);
                            }
                    }
                    else
                    {
                        var errorResponse = new
                        {
                            statusCode = 400,
                            code = -1,
                            message = "未登入不可購買點數商品",
                        };
                        return Ok(errorResponse);
                    }
                }

                db.SaveChanges();

                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "前往結帳成功",
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

        public class OutputTakeTime
        {
            public string takeDate { get; set; }
            public string takeTime { get; set; }
        }

        //CC-4 取得外帶自取時間
        [HttpGet]
        [Route("api/customer/getTakeTime")]
        public IHttpActionResult GetTakeTime()
        {
            try
            {
                List<OutputTakeTime> availableSlots = new List<OutputTakeTime>();
                DateTime now = DateTime.Now;

                // 當天的時間選項
                DateTime startTime = now.AddMinutes(30 - now.Minute % 15).AddSeconds(-now.Second); // 調整到下一個15分鐘整點
                DateTime endTimeToday = DateTime.Today.AddHours(21).AddMinutes(30); // 今天的21:30

                if (startTime <= DateTime.Today.AddHours(10))
                {
                    startTime = DateTime.Today.AddHours(10);
                }

                while (startTime <= endTimeToday)
                {
                    OutputTakeTime formattedTime = new OutputTakeTime();
                    formattedTime.takeDate = $"{startTime:yyyy-MM-dd(ddd)}".Replace("週", "");
                    formattedTime.takeTime = $"{startTime:HH:mm}";
                    availableSlots.Add(formattedTime);
                    startTime = startTime.AddMinutes(15);
                }

                if(availableSlots.Any())
                {
                    if (availableSlots.Last().takeTime == "21:15")
                    {
                        OutputTakeTime formattedTime = new OutputTakeTime();
                        formattedTime.takeDate = $"{startTime:yyyy-MM-dd(ddd)}".Replace("週", "");
                        formattedTime.takeTime = $"{endTimeToday:HH:mm}";
                        availableSlots.Add(formattedTime);
                    }
                }
                


                // 隔天和第三天的時間選項
                for (int day = 1; day <= 2; day++)
                {
                    DateTime startTimeNextDay = DateTime.Today.AddDays(day).AddHours(10); // 10:00開始
                    DateTime endTimeNextDay = DateTime.Today.AddDays(day).AddHours(21).AddMinutes(30); // 到21:30

                    while (startTimeNextDay <= endTimeNextDay)
                    {
                        OutputTakeTime formattedTime = new OutputTakeTime();
                        formattedTime.takeDate = $"{startTimeNextDay:yyyy-MM-dd(ddd)}".Replace("週", "");
                        formattedTime.takeTime = $"{startTimeNextDay:HH:mm}";
                        availableSlots.Add(formattedTime);
                        startTimeNextDay = startTimeNextDay.AddMinutes(15);
                    }
                }

                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "取得外帶自取時間成功",
                    data = availableSlots
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
