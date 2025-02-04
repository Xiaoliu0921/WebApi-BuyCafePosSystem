using BuyCafe.Models;
using BuyCafe.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using BuyCafe.Security;
using System.Web;

namespace BuyCafe.Controllers
{

    public class LinePayController : ApiController
    {
        DBModel db = new DBModel();
        private readonly LinePayService _linePayService;

        public LinePayController()
        {
            _linePayService = new LinePayService();
        }

        public class InputOrderLinePay
        {
            public int orderId { get; set; }
            public string guid { get; set; }
            public string invoice { get; set; }
            public string invoiceCarrier { get; set; }
            public string confirmUrl { get; set; }
            public string cancelUrl { get; set; }
        }

        // CP-2 送出訂單(LinePay)
        [HttpPost]
        [Route("api/customer/confirmOrderLinePay")]
        public async Task<IHttpActionResult> OrderReserve([FromBody] InputOrderLinePay input)
        {
            try
            {
                // 先檢查訂單是否存在
                var order = db.Order.FirstOrDefault(o => o.Id == input.orderId && o.Guid == input.guid);

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

                // 建立要發給LinePay的Api
                var products = order.Items.Select(oi => new ProductDto
                {
                    name = oi.Name,
                    quantity = oi.Quantity,
                    price = oi.Price,
                }).ToList();

                var packages = new PackageDto
                {
                    id = order.Id.ToString(),
                    amount = (int)order.TotalAmount,
                    products = products
                };

                List<PackageDto> packageDtos = new List<PackageDto>();
                packageDtos.Add(packages);

                var request = new LinePayRequestDto
                {
                    amount = (int)order.TotalAmount,
                    orderId = order.Id.ToString(),
                    packages = packageDtos,
                    redirectUrls = new RedirectUrlsDto
                    {
                        confirmUrl = input.confirmUrl,
                        cancelUrl = input.cancelUrl
                    }
                };

                var result = await _linePayService.ReservePaymentAsync(request);

                order.TransactionId = result.info.transactionId;
                db.SaveChanges();

                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "送出訂單成功",
                    data = new
                    {
                        paymentUrl = result.info.paymentUrl.web,
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

        public class InputOrderId
        { 
            public int orderId { get; set; }
            public string  guid { get; set; }
        }

        // CP-3 LinePay訂單確認
        [HttpPost]
        [JwtAuthFilter_C]
        [Route("api/customer/confirmLinePayRequest")]
        public async Task<IHttpActionResult> OrderConfirm([FromBody] InputOrderId input)
        {
            // 透過OrderId、Guid取得對應的訂單Amount、TransactionId
            var order = db.Order.FirstOrDefault(o => o.Id == input.orderId && o.Guid == input.guid);

            if (order != null)
            {
                // 將ConfirmRequestDto屬性賦值後當作方法參數
                var confirmRequest = new ConfirmRequestDto
                {
                    transactionId = (long)order.TransactionId,
                    amount = (int)order.TotalAmount
                };

                try
                {
                    var result = await _linePayService.GetPaymentStatusAsync(confirmRequest);
                    if (order.Type == TypeEnum.內用)
                    {
                        order.TypeAndNumber = $"內用{order.Table}桌";
                    }
                    else
                    {
                        int takeNumber = db.Order.Count(o => DbFunctions.TruncateTime(o.TakeTime) == DbFunctions.TruncateTime(order.TakeTime) && o.Type != TypeEnum.內用 && o.OrderStatus != OrderStatusEnum.點餐中) + 1;
                        order.TypeAndNumber = "外帶" + takeNumber.ToString("D3");
                    }
                    order.OrderStatus = OrderStatusEnum.準備中;

                    int payPoint = (int)order.Items.Where(oi => oi.Point.HasValue).Sum(oi => oi.Point.Value);
                    if(payPoint > 0)
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
                        message = "確認交易成功",
                        data = new
                        {
                            isPayMent = true
                        }
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
                        data = new
                        {
                            isPayMent = false,
                            errorMessage = ex.Message
                        }
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
                    message = "找不到對應的訂單"
                };

                return Ok(response);
            }
        }
    }
}
