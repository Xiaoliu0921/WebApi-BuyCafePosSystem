using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BuyCafe.Models;
using BuyCafe.Security;
using Microsoft.Ajax.Utilities;

namespace BuyCafe.Controllers
{
    public class FohSearchController : ApiController
    {
        DBModel db = new DBModel();

        //FS-0 取得今日訂單數量與頁數
        [HttpGet]
        [JwtAuthFilter_E]
        [Route("api/foh/getOrderCount/{orderStatus?}")]
        public IHttpActionResult GetOrderCount(string orderStatus)
        {
            try
            {
                if (orderStatus.IsNullOrWhiteSpace() || orderStatus == "全部訂單" || orderStatus == "0")
                {
                    //沒傳值或是要求"全部訂單"就回傳全部
                    int orderCount = db.Order.Count(o => DbFunctions.TruncateTime(o.TakeTime) == DbFunctions.TruncateTime(DateTime.Now)
                    && (o.OrderStatus == OrderStatusEnum.待結帳 || o.OrderStatus == OrderStatusEnum.準備中 || o.OrderStatus == OrderStatusEnum.待取餐));
                    int page = (int)(orderCount / 9) + 1;
                    var response = new
                    {
                        statusCode = 200,
                        code = 0,
                        message = "取得訂單數與頁數成功",
                        data = new
                        {
                            orderCount = orderCount,
                            pageCount = page
                        }
                    };
                    return Ok(response);

                }
                else
                {
                    //判斷傳入的"訂單狀態"是否正確
                    if (Enum.TryParse(orderStatus, out OrderStatusEnum orderStatusEnum))
                    {
                        int orderCount = db.Order.Count(o => DbFunctions.TruncateTime(o.TakeTime) == DbFunctions.TruncateTime(DateTime.Today) && o.OrderStatus == orderStatusEnum);
                        int page = (int)(orderCount / 9) + 1;
                        var response = new
                        {
                            statusCode = 200,
                            code = 0,
                            message = "取得訂單數與頁數成功",
                            data = new
                            {
                                orderCount = orderCount,
                                pageCount = page
                            }
                        };
                        return Ok(response);
                    }
                    else
                    {
                        var errorResponse = new
                        {
                            statusCode = 400,
                            code = -1,
                            message = "選擇之訂單狀態異常"
                        };
                        return Ok(errorResponse);
                    }
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

        //FS-0 取得今日訂單數量與頁數
        [HttpGet]
        [JwtAuthFilter_E]
        [Route("api/foh/getOrderCount/")]
        public IHttpActionResult GetOrderCountAll()
        {
            try
            {
                //沒傳值或是要求"全部訂單"就回傳全部
                int orderCount = db.Order.Count(o => DbFunctions.TruncateTime(o.TakeTime) == DbFunctions.TruncateTime(DateTime.Today)
                && (o.OrderStatus == OrderStatusEnum.待結帳 || o.OrderStatus == OrderStatusEnum.準備中 || o.OrderStatus == OrderStatusEnum.待取餐));
                int page = (int)(orderCount / 9) + 1;
                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "取得訂單數與頁數成功",
                    data = new
                    {
                        orderCount = orderCount,
                        pageCount = page,
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

        //FS-1 取得今日訂單數量與頁數
        [HttpGet]
        [JwtAuthFilter_E]
        [Route("api/foh/getOrder")]
        public IHttpActionResult GetOrder([FromUri]string page = null, [FromUri] string orderStatus = null, [FromUri] string type = null, [FromUri] string orderBy = null, [FromUri] string search = null)
        {
            try
            {
                var query = db.Order.AsQueryable();

                //只需要顯示今日的訂單
                query = query.Where(o => DbFunctions.TruncateTime(o.TakeTime) == DbFunctions.TruncateTime(DateTime.Today));

                //判斷各個傳入值-orderStatus
                if (orderStatus.IsNullOrWhiteSpace() || orderStatus == "全部訂單" || orderStatus == "0")
                {
                    query = query.Where(o => o.OrderStatus == OrderStatusEnum.待結帳 ||
                    o.OrderStatus == OrderStatusEnum.準備中 ||
                    o.OrderStatus == OrderStatusEnum.待取餐);
                }
                else
                {
                    //orderStatus不為null不為空不為"全部訂單"不為"0"就檢查是否正確
                    if (Enum.TryParse(orderStatus, out OrderStatusEnum orderStatusEnum))
                    {
                        query = query.Where(o => o.OrderStatus == orderStatusEnum);
                    }
                    else
                    {
                        var errorResponse = new
                        {
                            statusCode = 400,
                            code = -1,
                            message = "輸入之訂單狀態異常"
                        };
                        return Ok(errorResponse);
                    }
                }


                //判斷各個傳入值-typeEnum
                if (type.IsNullOrWhiteSpace() || type == "0" || type == "全部訂單")
                {
                    //query = query.Where(o => o.Type == TypeEnum.內用 || o.Type == TypeEnum.外帶 || o.Type == TypeEnum.預約自取);
                }
                else
                {
                    //typeEnum不為null不為空不為"全部訂單"不為"0"就檢查是否正確
                    if (Enum.TryParse(type, out TypeEnum Enum_typeEnum))
                    {
                        if(Enum_typeEnum==TypeEnum.內用)
                        {
                            query = query.Where(o => o.Type == Enum_typeEnum);
                        }
                        else if(Enum_typeEnum==TypeEnum.外帶|| Enum_typeEnum==TypeEnum.預約自取)
                        {
                            query=query.Where(o=>o.Type!=TypeEnum.內用);
                        }
                    }
                    else
                    {
                        var errorResponse = new
                        {
                            statusCode = 400,
                            code = -1,
                            message = "輸入之用餐類型異常"
                        };
                        return Ok(errorResponse);
                    }
                }

                //判斷各個傳入值-orderBy
                if (orderBy == "時間越早優先")
                {
                    query = query.OrderBy(o => o.TakeTime);
                }
                else if (orderBy == "時間越晚優先")
                {
                    query = query.OrderByDescending(o => o.TakeTime);
                }
                else
                {
                    query = query.OrderBy(o => o.TakeTime);
                }

                //判斷各個傳入值-search
                if (!search.IsNullOrWhiteSpace())
                {
                    query = query.Where(o => o.CustomerPhone.Contains(search) ||
                    o.TypeAndNumber.Contains(search)
                    );
                }



                //判斷各個傳入值-page
                if (page.IsNullOrWhiteSpace())
                {
                    page = "1";
                }

                int pageNum = Convert.ToInt32(page);
                int pageSize = 9;
                int totalCount = query.Count();
                if (totalCount / pageSize + 1 < pageNum)
                {
                    pageNum = 1;
                }

                //var result = query.Skip((pageNum - 1) * pageSize).Take(pageSize)
                //    .Select(o => new
                //    {
                //        orderId = o.Id,
                //        orderStatus = o.OrderStatus.ToString(),
                //        typeAndNumber = o.TypeAndNumber,
                //        phone = o.CustomerPhone,
                //        time = o.Type == TypeEnum.預約自取 ? $"{o.TakeTime.Value:HH/mm}取餐" : $"{o.TakeTime.Value:HH/mm}點餐",
                //        totalAmount = o.TotalAmount
                //    }).ToList();

                var result = query.Skip((pageNum - 1) * pageSize).Take(pageSize)
                            .Select(o => new
                            {
                                o.Id,
                                o.OrderStatus,
                                o.TypeAndNumber,
                                o.CustomerPhone,
                                o.TakeTime,
                                o.Type,
                                o.TotalAmount
                            }).ToList();

                // 然後在內存中進行格式化操作
                var formattedResult = result.Select(o => new
                {
                    orderId = o.Id,
                    orderStatus = o.OrderStatus.ToString(),
                    typeAndNumber = o.TypeAndNumber,
                    phone = o.CustomerPhone,
                    time = o.Type == TypeEnum.預約自取 ? $"{o.TakeTime.Value:HH:mm}取餐" : $"{o.TakeTime.Value:HH:mm}點餐",
                    totalAmount = o.TotalAmount
                }).ToList();


                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "取得訂單總覽成功",
                    data = formattedResult
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

        //FS-2 取得單一訂單資訊
        [HttpGet]
        [JwtAuthFilter_E]
        [Route("api/foh/getOrderDetail/{orderId}")]
        public IHttpActionResult GetOrderDetail(string orderId)
        {
            try
            {
                int orderIdNum = Convert.ToInt32(orderId);
                var order = db.Order.FirstOrDefault(o => o.Id == orderIdNum);
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

                var items = order.Items.Select(oi => new
                {
                    name = oi.Name,
                    serving = oi.Quantity,
                    price = oi.Price,
                    customization = oi.Customization.Replace(",", "/")
                });



                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "取得單一訂單資訊成功",
                    data = new
                    {
                        orderId=orderIdNum,
                        typeAndNumber = order.TypeAndNumber,
                        orderStatus = order.OrderStatus.ToString(),
                        phone = order.CustomerPhone,
                        time = order.Type == TypeEnum.預約自取 ? $"{order.TakeTime.Value:HH:mm}取餐" : $"{order.TakeTime.Value:HH:mm}點餐",
                        orderNumber = order.Id.ToString("D7"), //訂單編號(字串)
                        count = order.Items.Count,
                        totalAmount = order.TotalAmount,
                        items = items
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


        //FS-1 取得今日訂單數量與頁數
        [HttpGet]
        [JwtAuthFilter_E]
        [Route("api/foh/getOrderNoPaging")]
        public IHttpActionResult GetOrderNoPaging([FromUri] string page = null, [FromUri] string orderStatus = null, [FromUri] string type = null, [FromUri] string orderBy = null, [FromUri] string search = null)
        {
            try
            {
                var query = db.Order.AsQueryable();

                //只需要顯示今日的訂單
                query = query.Where(o => DbFunctions.TruncateTime(o.TakeTime) == DbFunctions.TruncateTime(DateTime.Today));

                //判斷各個傳入值-orderStatus
                if (orderStatus.IsNullOrWhiteSpace() || orderStatus == "全部訂單" || orderStatus == "0")
                {
                    query = query.Where(o => o.OrderStatus == OrderStatusEnum.待結帳 ||
                    o.OrderStatus == OrderStatusEnum.準備中 ||
                    o.OrderStatus == OrderStatusEnum.待取餐);
                }
                else
                {
                    //orderStatus不為null不為空不為"全部訂單"不為"0"就檢查是否正確
                    if (Enum.TryParse(orderStatus, out OrderStatusEnum orderStatusEnum))
                    {
                        query = query.Where(o => o.OrderStatus == orderStatusEnum);
                    }
                    else
                    {
                        var errorResponse = new
                        {
                            statusCode = 400,
                            code = -1,
                            message = "輸入之訂單狀態異常"
                        };
                        return Ok(errorResponse);
                    }
                }


                //判斷各個傳入值-typeEnum
                if (type.IsNullOrWhiteSpace() || type == "0" || type == "全部訂單")
                {
                    //query = query.Where(o => o.Type == TypeEnum.內用 || o.Type == TypeEnum.外帶 || o.Type == TypeEnum.預約自取);
                }
                else
                {
                    //typeEnum不為null不為空不為"全部訂單"不為"0"就檢查是否正確
                    if (Enum.TryParse(type, out TypeEnum Enum_typeEnum))
                    {
                        if (Enum_typeEnum == TypeEnum.內用)
                        {
                            query = query.Where(o => o.Type == Enum_typeEnum);
                        }
                        else if (Enum_typeEnum == TypeEnum.外帶 || Enum_typeEnum == TypeEnum.預約自取)
                        {
                            query = query.Where(o => o.Type != TypeEnum.內用);
                        }
                    }
                    else
                    {
                        var errorResponse = new
                        {
                            statusCode = 400,
                            code = -1,
                            message = "輸入之用餐類型異常"
                        };
                        return Ok(errorResponse);
                    }
                }

                //判斷各個傳入值-orderBy
                if (orderBy == "時間越早優先")
                {
                    query = query.OrderBy(o => o.TakeTime);
                }
                else if (orderBy == "時間越晚優先")
                {
                    query = query.OrderByDescending(o => o.TakeTime);
                }
                else
                {
                    query = query.OrderBy(o => o.TakeTime);
                }

                //判斷各個傳入值-search
                if (!search.IsNullOrWhiteSpace())
                {
                    query = query.Where(o => o.CustomerPhone.Contains(search) ||
                    o.TypeAndNumber.Contains(search)
                    );
                }



                ////判斷各個傳入值-page
                //if (page.IsNullOrWhiteSpace())
                //{
                //    page = "1";
                //}

                //int pageNum = Convert.ToInt32(page);
                //int pageSize = 9;
                //int totalCount = query.Count();
                //if (totalCount / pageSize + 1 < pageNum)
                //{
                //    pageNum = 1;
                //}

                //var result = query.Skip((pageNum - 1) * pageSize).Take(pageSize)
                //    .Select(o => new
                //    {
                //        orderId = o.Id,
                //        orderStatus = o.OrderStatus.ToString(),
                //        typeAndNumber = o.TypeAndNumber,
                //        phone = o.CustomerPhone,
                //        time = o.Type == TypeEnum.預約自取 ? $"{o.TakeTime.Value:HH/mm}取餐" : $"{o.TakeTime.Value:HH/mm}點餐",
                //        totalAmount = o.TotalAmount
                //    }).ToList();

                var result = query
                            .Select(o => new
                            {
                                o.Id,
                                o.OrderStatus,
                                o.TypeAndNumber,
                                o.CustomerPhone,
                                o.TakeTime,
                                o.Type,
                                o.TotalAmount
                            }).ToList();

                // 然後在內存中進行格式化操作
                var formattedResult = result.Select(o => new
                {
                    orderId = o.Id,
                    orderStatus = o.OrderStatus.ToString(),
                    typeAndNumber = o.TypeAndNumber,
                    phone = o.CustomerPhone,
                    time = o.Type == TypeEnum.預約自取 ? $"{o.TakeTime.Value:HH:mm}取餐" : $"{o.TakeTime.Value:HH:mm}點餐",
                    totalAmount = o.TotalAmount
                }).ToList();


                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "取得訂單總覽成功",
                    data = formattedResult
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
