using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Web.Http;
using System.Web.Services.Description;
using BuyCafe.Models;
using Newtonsoft.Json;
using BuyCafe.Security;
using System.Web;

namespace BuyCafe.Controllers
{
    public class CustomerMenuController : ApiController
    {
        private DBModel db = new DBModel();

        //CM-1 取得菜單類別
        [HttpGet]
        [Route("api/customer/getMenuCategory")]
        public IHttpActionResult GetMenuCategory()
        {
            try
            {
                var categories = db.ProductCategory
                    .Where(pc => pc.Id != 8) //分類Id8的是點數兌換
                    .Select(pc => new
                    {
                        categoryId = pc.Id,
                        category = pc.Name
                    }).ToList();

                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "取得菜單類別成功",
                    data = categories
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    StatusCode = 400,
                    Code = -1,
                    Message = "其他異常",
                    Data = ex.Message // 返回異常訊息
                };

                return BadRequest(JsonConvert.SerializeObject(response));
            }

        }

        //CM-2 取得菜單品項(該類別)
        [HttpGet]
        [Route("api/customer/getMenuItem/{categoryId}")]

        public IHttpActionResult GetMenuItem(int? categoryId)
        {


            if (categoryId == null || !(db.ProductCategory.Any(input => input.Id == categoryId)))
            {
                categoryId = db.ProductCategory.FirstOrDefault()?.Id;
            }

            try
            {
                var items = db.Product.Where(item => item.CategoryId == categoryId)
                                    .Select(item => new
                                    {
                                        productId = item.Id,
                                        name = item.Name,
                                        productImagePath = item.ImagePath,
                                        isPoint = item.isPoint,
                                        description = item.Description,
                                        price = item.Price
                                    })
                                    .ToList();



                var response = new
                {
                    StatusCode = 200,
                    Code = 0,
                    Message = "取得菜單成功",
                    Data = items
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


        public class MenuItemDto
        {
            public int productId { get; set; }
            public string name { get; set; }
            public string imagePath { get; set; }
            public bool isPoint { get; set; }
            public string description { get; set; }
            public int price { get; set; }
            public int? orderQuantity { get; set; } = null; // 可修改的屬性
        }

        public class CategoryDto
        {
            public int category { get; set; }
            public string categoryName { get; set; }
            public List<MenuItemDto> categoryItem { get; set; }
        }


        //CM-2 取得菜單品項
        [HttpGet]
        [Route("api/customer/getMenuItem/")]

        public IHttpActionResult GetMenuItem(int? orderId = null, string guid = null)
        {
            try
            {
                //var menuItems = db.ProductCategory.Where(pc => pc.Id != 8)
                //                              .Select(pc => new CategoryDto
                //                              {
                //                                  category = pc.Id,
                //                                  categoryName = pc.Name,
                //                                  categoryItem = (List<MenuItemDto>)pc.Products.Where(p => p.IsAvailable == true)
                //                                                           .Select(p => new MenuItemDto
                //                                                           {
                //                                                               productId = p.Id,
                //                                                               name = p.Name,
                //                                                               imagePath = p.ImagePath,
                //                                                               isPoint = p.isPoint,
                //                                                               description = p.Description,
                //                                                               price = p.Price
                //                                                           }).ToList()
                //                              }).ToList();

                // 獲取所有菜單品項，排除 ID 為 8 的分類
                var menuItems = db.ProductCategory
                    .Where(pc => pc.Id != 8)
                    .Select(pc => new CategoryDto
                    {
                        category = pc.Id,
                        categoryName = pc.Name,
                        categoryItem = pc.Products
                            .Where(p => p.IsAvailable)
                            .Select(p => new MenuItemDto
                            {
                                productId = p.Id,
                                name = p.Name,
                                imagePath = p.ImagePath,
                                isPoint = p.isPoint,
                                description = p.Description,
                                price = p.Price,
                            }).ToList() // 這裡使用 .ToList()，直接轉換為 List<MenuItemDto>
                    }).ToList(); // 這裡也使用 .ToList()，直接轉換為 List<CategoryDto>


                if (orderId.HasValue && !string.IsNullOrEmpty(guid))
                {
                    var order = db.Order.FirstOrDefault(o => o.Id == orderId && o.Guid == guid);

                    if (order != null)
                    {
                        // 將訂單中的產品數量更新到菜單項目中
                        foreach (var orderItem in order.Items)
                        {
                            // 尋找對應的產品
                            foreach (var category in menuItems)
                            {
                                foreach (var product in category.categoryItem)
                                {
                                    // 如果找到對應的產品，更新數量
                                    if (product.productId == orderItem.ProductId)
                                    {
                                        product.orderQuantity = orderItem.Quantity; // 更新訂單數量
                                    }
                                }
                            }
                        }
                    }
                }
                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "取得菜單成功",
                    data = menuItems
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

        //CM-3 取得單一餐點資訊
        [HttpGet]
        [JwtAuthFilter_C]
        [Route("api/customer/getProduct/{productId}")]
        public IHttpActionResult GetProduct(int productId)
        {
            try
            {
                var product = db.Product.FirstOrDefault(p => p.Id == productId);

                if (product == null)
                {
                    var errorResponse = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "找不到該商品",
                    };
                    return Ok(errorResponse);
                }

                if (product.CategoryId != 8)
                {
                    var productDetail = new
                    {
                        name = product.Name,
                        categoryId = product.CategoryId,
                        category = product.MyCategory.Name,
                        productImagePath = product.ImagePath,
                        isPoint = product.isPoint,
                        description = product.Description,
                        price = product.Price,
                        customization = product.Customizations.Select(c => c.CustomizationEnum).ToList()
                    };

                    var response = new
                    {
                        statusCode = 200,
                        code = 0,
                        message = "取得單一商品資訊成功",
                        data = productDetail
                    };
                    return Ok(response);
                }
                else
                {
                    bool isLogin = (bool)HttpContext.Current.Items["IsAuthenticated"];

                    if (isLogin)
                    {
                        var productDetail = new
                        {
                            name = product.Name,
                            categoryId = product.CategoryId,
                            category = product.MyCategory.Name,
                            productImagePath = product.ImagePath,
                            isDiscount = product.isPoint,
                            description = product.Description,
                            point = product.Point,
                            customization = product.Customizations.Select(c => c.CustomizationEnum).ToList()
                        };

                        var response = new
                        {
                            statusCode = 200,
                            code = 0,
                            message = "取得單一點數商品資訊成功",
                            isLogin = isLogin,
                            data = productDetail
                        };
                        return Ok(response);
                    }
                    else
                    {
                        var response = new
                        {
                            statusCode = 401,
                            code = -1,
                            message = "未登入",
                            isLogin = isLogin,
                        };
                        return Ok(response);
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

        public class InputProduct
        {
            public string guid { get; set; }
            public int orderId { get; set; }
            public int productId { get; set; }
            public List<CustomizationOption> customization { get; set; } // Customization 列表可以為 null
            public int serving { get; set; }
        }

        public class CustomizationOption
        {
            public string options { get; set; }  // Options 可以為 null
            public int extraPrice { get; set; } = 0;
        }

        //CM-4 加入購物車(POST)
        [HttpPost]
        [JwtAuthFilter_C]
        [Route("api/customer/AddItem")]
        public IHttpActionResult PostAddItem([FromBody] InputProduct input)
        {
            try
            {
                //先檢查該訂單是否存在
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

                if (input.serving <= 0 || input.serving >= 100)
                {
                    var errorResponse = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "份數異常(低於1或超過99份)",
                    };
                    return Ok(errorResponse);
                }


                //再來抓商品資訊
                var product = db.Product.FirstOrDefault(p => p.Id == input.productId);

                if (product == null)
                {
                    var errorResponse = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "找不到該商品資訊",
                    };
                    return Ok(errorResponse);
                }

                //訂單跟商品都沒問題，先處理完客製化選項字串
                string customizationOptionsString = "";
                int extraAmount = 0;

                if (input.customization != null && input.customization.Count > 0)
                {
                    // 使用 LINQ 進行簡化處理
                    customizationOptionsString = string.Join(",", input.customization
                        .Where(co => !string.IsNullOrEmpty(co.options))  // 過濾掉為 null 或空的選項
                        .Select(co => co.options));  // 取得所有選項

                    customizationOptionsString = customizationOptionsString.Replace("要不要鮮奶油 : 是", "加鮮奶油")
                                                .Replace("更換燕麥奶 : 是", "換燕麥奶")
                                                .Replace(",要不要鮮奶油 :否", "")
                                                .Replace(",更換燕麥奶 : 否", "")
                                                .Replace("要不要鮮奶油 :否", "")
                                                .Replace("更換燕麥奶 : 否", "");

                    extraAmount = input.customization
                        .Sum(co => co.extraPrice);  // 計算所有額外價格的總和
                }

                //點數兌換商品額外處理
                if (product.CategoryId == 8)
                {
                    if ((bool)HttpContext.Current.Items["IsAuthenticated"])
                    {
                        string phoneNumber = HttpContext.Current.Items["PhoneNumber"].ToString();
                        var member = db.Member.FirstOrDefault(m => m.Phone == phoneNumber);
                        int payPoint = (int)order.Items.Where(oi => oi.Point.HasValue).Sum(oi => oi.Point.Value);
                        if (member != null)
                        {
                            int memberPoint = member.Point;
                            if (memberPoint < (payPoint + product.Point))
                            {
                                var errorResponse = new
                                {
                                    statusCode = 400,
                                    code = -1,
                                    message = "點數不足",
                                };
                                return Ok(errorResponse);
                            }

                            var orderPointItem = new OrderItem
                            {
                                OrderId = input.orderId,
                                ProductId = input.productId,
                                Name = "(點數兌換)" + product.Name,
                                ImagePath = product.ImagePath,
                                Customization = customizationOptionsString,
                                Quantity = input.serving,
                                Price = product.Price,
                                Point = product.Point,
                            };

                            db.OrderItem.Add(orderPointItem);
                            db.SaveChanges();
                            var response2 = new
                            {
                                statusCode = 200,
                                code = 0,
                                message = "加入購物車成功",
                            };
                            return Ok(response2);


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


                //檢查該訂單中有沒有一模一樣的商品
                var existingOrderItem = order.Items.FirstOrDefault(oi => oi.ProductId == input.productId && oi.Customization == customizationOptionsString);

                if (existingOrderItem != null)
                {
                    order.TotalAmount += ((product.Price + extraAmount) * input.serving); //訂單計算$$要算份數*單價
                    existingOrderItem.Quantity += input.serving;
                    db.SaveChanges();
                }
                else
                {
                    //客製化處理完後就將商品存進orderItem

                    var orderItem = new OrderItem
                    {
                        OrderId = input.orderId,
                        ProductId = input.productId,
                        Name = product.Name,
                        ImagePath = product.ImagePath,
                        Customization = customizationOptionsString,
                        Quantity = input.serving,
                        Price = (product.Price + extraAmount)  //單價
                    };

                    db.OrderItem.Add(orderItem);
                    order.TotalAmount += (orderItem.Price * orderItem.Quantity); //訂單計算$$要算份數*單價
                    db.SaveChanges();
                }


                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "加入購物車成功",
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


        //CM-5 取得點數菜單類別
        [HttpGet]
        [JwtAuthFilter_C]
        [Route("api/customer/getPointMenuCategory")]
        public IHttpActionResult GetPointMenuCategory()
        {
            try
            {
                var categories = db.ProductCategory
                    .Where(pc => pc.Id == 8) //分類Id8的是點數兌換
                    .Select(pc => new
                    {
                        categoryId = pc.Id,
                        category = pc.Name
                    }).ToList();

                //判斷是否有登入
                bool isLogin = (bool)HttpContext.Current.Items["IsAuthenticated"];

                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "取得點數菜單類別成功",
                    isLogin = isLogin,
                    data = categories
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    StatusCode = 400,
                    Code = -1,
                    Message = "其他異常",
                    Data = ex.Message // 返回異常訊息
                };

                return BadRequest(JsonConvert.SerializeObject(response));
            }
        }

        //CM-6 取得點數菜單品項
        [HttpGet]
        [JwtAuthFilter_C]
        [Route("api/customer/getPointMenuItem")]
        public IHttpActionResult GetPointMenuItem()
        {
            try
            {

                var items = db.ProductCategory.Where(pc => pc.Id == 8)
                                              .Select(pc => new
                                              {
                                                  category = pc.Id,
                                                  categoryName = pc.Name,
                                                  categoryItem = pc.Products.Where(p => p.IsAvailable == true)
                                                                           .Select(p => new
                                                                           {
                                                                               productId = p.Id,
                                                                               name = p.Name,
                                                                               imagePath = p.ImagePath,
                                                                               isDiscount = p.isPoint,
                                                                               description = p.Description,
                                                                               point = p.Point
                                                                           })
                                              }).ToList();

                //判斷是否有登入
                bool isLogin = (bool)HttpContext.Current.Items["IsAuthenticated"];

                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "取得點數菜單品項成功",
                    isLogin = isLogin,
                    data = items
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    StatusCode = 400,
                    Code = -1,
                    Message = "其他異常",
                    Data = ex.Message // 返回異常訊息
                };

                return BadRequest(JsonConvert.SerializeObject(response));
            }
        }

        //CM-7 搜尋商品
        [HttpGet]
        [Route("api/customer/getSearchMenuItem/{searchString}")]
        public IHttpActionResult GetSearchMenuItem(string searchString)
        {
            try
            {
                var items = db.ProductCategory.Where(pc => pc.Id != 8 && pc.Products.Any(p => p.Name.Contains(searchString)))
                                              .Select(pc => new
                                              {
                                                  category = pc.Id,
                                                  categoryName = pc.Name,
                                                  categoryItem = pc.Products.Where(p => p.IsAvailable == true && p.Name.Contains(searchString))
                                                                           .Select(p => new
                                                                           {
                                                                               productId = p.Id,
                                                                               name = p.Name,
                                                                               imagePath = p.ImagePath,
                                                                               isPoint = p.isPoint,
                                                                               description = p.Description,
                                                                               price = p.Price
                                                                           })
                                              }).ToList();

                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "取得搜尋之品項成功",
                    data = items
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    StatusCode = 400,
                    Code = -1,
                    Message = "其他異常",
                    Data = ex.Message // 返回異常訊息
                };

                return BadRequest(JsonConvert.SerializeObject(response));
            }
        }



    }
}
