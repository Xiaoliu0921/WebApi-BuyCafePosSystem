using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BuyCafe.Models;
using static BuyCafe.Controllers.TestCRUDController;

namespace BuyCafe.Controllers
{
    public class TestCRUDController : ApiController
    {
        // GET: api/TestCRUD
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }


        // GET: api/TestCRUD/5
        public string Get(int id)
        {
            return "value";
        }

        [HttpGet]
        [Route("api/Test/")]
        public IHttpActionResult TestApi()
        {
            return Ok("Test");
        }


        private readonly DBModel _context;

        public TestCRUDController()
        {
            _context = new DBModel(); // 假設你用的是無參數的 DBModel 構造函數
        }

        [HttpPost]
        [Route("api/TestCRUD/edit087")]
        public IHttpActionResult PostEdit087([FromBody] string value)
        {
            int orderId=Convert.ToInt32(value);
            var order = _context.Order.FirstOrDefault(o => o.Id == orderId);
            if(order!=null)
            {
                order.TypeAndNumber = "外帶078";
                _context.SaveChanges();
            }
            return Ok("OK");

        }


        // POST: api/TestCRUD
        [HttpPost]
        [Route("api/TestCRUD/AddProductCategory")]
        public IHttpActionResult PostAddProductCategory([FromBody] string value)
        {
            var newCategory = new ProductCategory
            {
                Name = value,
                CreateDate = DateTime.Now
            };

            _context.ProductCategory.Add(newCategory);
            _context.SaveChanges();

            newCategory.SortValue = newCategory.Id;
            _context.SaveChanges();

            return Ok("OK");

        }

        public class InputProduct
        {
            public int categoryId { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public int price { get; set; }

        }

        // POST: api/TestCRUD
        [HttpPost]
        [Route("api/TestCRUD/AddProduct")]
        public IHttpActionResult PostAddProduct([FromBody] InputProduct inputProduct)
        {

            //var categoryExists = _context.ProductCategory.Any(c => c.Id == inputProduct.categoryId);
            //if (!categoryExists)
            //{
            //    return BadRequest("Invalid CategoryId.");
            //}



            //var newProduct = new Product
            //{
            //    CategoryId = inputProduct.categoryId,
            //    Name = inputProduct.name,
            //    Description = inputProduct.description,
            //    Price = inputProduct.price,
            //};

            //_context.Product.Add(newProduct);
            //_context.SaveChanges();

            //// 設定 SortValue 在 Id 生成後
            //newProduct.SortValue = newProduct.Id;
            //_context.SaveChanges();

            //return Ok("OK");


            try
            {
                var newProduct = new Product
                {
                    CategoryId = inputProduct.categoryId,
                    Name = inputProduct.name,
                    Description = inputProduct.description,
                    Price = inputProduct.price,
                };

                _context.Product.Add(newProduct);
                _context.SaveChanges();

                // 設定 SortValue 在 Id 生成後
                newProduct.SortValue = newProduct.Id;
                _context.SaveChanges();

                return Ok("OK");
            }
            catch (DbEntityValidationException ex)
            {
                // 捕捉驗證錯誤
                var errors = ex.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors)
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                    .ToList();

                return BadRequest(string.Join("; ", errors));
            }

        }

        public class InputProductCustomization
        {
            public int productId { get; set; }
            public string title { get; set; }
            public int customizationEnum { get; set; }
        }


        // POST: api/TestCRUD
        [HttpPost]
        [Route("api/TestCRUD/AddProductCustomization")]
        public IHttpActionResult PostAddProductCustom([FromBody] InputProductCustomization inputProductCustomization)
        {

            var newCustomization = new ProductCustomization
            {
                ProductId= inputProductCustomization.productId,
                Title= inputProductCustomization.title,
                CustomizationEnum= inputProductCustomization.customizationEnum
            };

            _context.ProductCustomization.Add(newCustomization);
            _context.SaveChanges();

            return Ok("OK");

        }



        // PUT: api/TestCRUD/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/TestCRUD/5
        public void Delete(int id)
        {
        }
    }
}
