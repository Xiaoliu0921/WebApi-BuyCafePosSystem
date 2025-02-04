using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BuyCafe.Models;

namespace BuyCafe.Controllers
{
    public class AdminDatabaseController : ApiController
    {
        DBModel db = new DBModel();

        [HttpGet]
        [Route("api/Adminn/getOrder")]
        public IHttpActionResult GetOrder()
        {
            var outputs=db.Order.ToList();
            return Ok(outputs);
        }


        [HttpGet]
        [Route("api/Adminn/getEmployee")]
        public IHttpActionResult GetEmployee()
        {
            var outputs = db.Employee.ToList();
            return Ok(outputs);
        }

        [HttpGet]
        [Route("api/Adminn/getMember")]
        public IHttpActionResult GetMember()
        {
            var outputs = db.Member.ToList();
            return Ok(outputs);
        }

        [HttpGet]
        [Route("api/Adminn/getOrderItem")]
        public IHttpActionResult GetOrderItem()
        {
            var outputs = db.OrderItem.ToList();
            return Ok(outputs);
        }

        [HttpGet]
        [Route("api/Adminn/getOwner")]
        public IHttpActionResult GetOwner()
        {
            var outputs = db.Owner.ToList();
            return Ok(outputs);
        }

        [HttpGet]
        [Route("api/Adminn/getProduct")]
        public IHttpActionResult GetProduct()
        {
            var outputs = db.Product.ToList();
            return Ok(outputs);
        }

        [HttpGet]
        [Route("api/Adminn/getProductCategory")]
        public IHttpActionResult GetProductCategory()
        {
            var outputs = db.ProductCategory.ToList();
            return Ok(outputs);
        }

        [HttpGet]
        [Route("api/Adminn/getProductCustomization")]
        public IHttpActionResult GetProductCustomization()
        {
            var outputs = db.ProductCustomization.ToList();
            return Ok(outputs);
        }



    }
}
