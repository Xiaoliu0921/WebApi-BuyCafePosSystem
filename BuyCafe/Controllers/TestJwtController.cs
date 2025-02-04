using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using BuyCafe.Security;

namespace BuyCafe.Controllers
{
    public class TestJwtController : ApiController
    {

        [HttpGet]
        [Route("api/TestGetToken_C")]
        public IHttpActionResult GetTokenC()
        {
            JwtAuthUtil jwtAuthUtil = new JwtAuthUtil();
            string token=jwtAuthUtil.GenerateToken_Customer("0912345678");

            var response = new
            {
                StatusCode = 200,
                Code = 0,
                Message = "取得測試顧客Token成功",
                Data = token
            };

            return Ok(response);

        }


        [HttpGet]
        [Route("api/TestGetToken_E")]
        public IHttpActionResult GetTokenE()
        {
            JwtAuthUtil jwtAuthUtil = new JwtAuthUtil();
            string token = jwtAuthUtil.GenerateToken_Employee("Green",Models.IdentityEnum.內場);

            var response = new
            {
                StatusCode = 200,
                Code = 0,
                Message = "取得測試店員Token成功",
                Data = token
            };

            return Ok(response);

        }

        [HttpGet]
        [Security.JwtAuthFilter_C]
        [Route("api/TestJwt_C")]
        // GET: api/Test/5
        public IHttpActionResult GetC()
        {
            if ((bool)HttpContext.Current.Items["IsAuthenticated"])
            {
                return Ok("驗證成功");
            }
            else
            {
                return Ok("驗證失敗");
            }
        }

        [HttpGet]
        [Security.JwtAuthFilter_E]
        [Route("api/TestJwt_E")]
        // GET: api/Test/5
        public IHttpActionResult GetE()
        {
            try
            {
                return Ok("驗證成功");
            }
            catch (Exception ex)
            {
                return Ok("驗證失敗");
            }
        }

    }
}
