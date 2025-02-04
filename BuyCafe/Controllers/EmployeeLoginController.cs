using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BuyCafe.Models;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using System.Globalization;
using System.Web.Services.Description;
using System.Security.Principal;
using BuyCafe.Security;

namespace BuyCafe.Controllers
{
    public class EmployeeLoginController : ApiController
    {
        DBModel db = new DBModel();


        #region 加密用Methods

        // Argon2 加密
        //產生 Salt 功能
        private byte[] CreateSalt()
        {
            var buffer = new byte[16];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(buffer);
            return buffer;
        }
        // Hash 處理加鹽的密碼功能
        private byte[] HashPassword(string password, byte[] salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));

            //底下這些數字會影響運算時間，而且驗證時要用一樣的值
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = 4; // 4 核心就設成 8
            argon2.Iterations = 2; // 迭代運算次數
            argon2.MemorySize = 512 * 1024;

            return argon2.GetBytes(16);
        }

        //驗證
        private bool VerifyHash(string password, byte[] salt, byte[] hash)
        {
            var newHash = HashPassword(password, salt);
            return hash.SequenceEqual(newHash); // LINEQ
        }

        #endregion

        #region 建立帳號用api
        //建立帳號用的api的input
        public class InputRegisterAccount
        {
            public int Identity { get; set; }
            public string Account { get; set; }
            public string Password { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Birthday { get; set; } = "1995/01/01";

        }

        //建立帳號用的後端用api
        [HttpPost]
        [Route("api/Employee/RegisterEmployee")]
        public string PostRegisterEmployee([FromBody]InputRegisterAccount input)
        {
            //檢查有無重複帳號
            if (db.Employee.Any(e => e.Account == input.Account))
            {
                return "已有人註冊過此帳號";
            }
            else
            {
                ////Hash 加鹽加密
                //var salt = CreateSalt();
                //string saltStr = Convert.ToBase64String(salt); //將 byte 改回字串存回資料表
                //var hash = HashPassword(input.Password, salt);
                //string hashPassword = Convert.ToBase64String(hash);

                DateTime date = DateTime.ParseExact(input.Birthday, "yyyy/MM/dd", CultureInfo.InvariantCulture);

                var employee = new Employee
                {
                    Account = input.Account,
                    //Password = hashPassword,
                    Password = input.Password,
                    Name = input.Name,
                    Email = input.Email,
                    Phone = input.Phone,
                    //Salt = saltStr,
                    Identity = (IdentityEnum)input.Identity,
                    Birthday = date
                };
                db.Employee.Add(employee);
                db.SaveChanges();

                return "OK";
            }
        }

        #endregion


        //EL-1登入用的input
        public class InputLogin
        {
            public string account { get; set; }
            public string password { get; set; }
        }


        //EL-1
        [HttpPost]
        [Route("api/employee/login")]
        public IHttpActionResult PostEmployeeLogin([FromBody]InputLogin input)
        {
            try
            {
                var employee = db.Employee.FirstOrDefault(e => e.Account == input.account);

                if (employee == null)
                {
                    var errorResponse = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "帳密錯誤"
                    };
                    return Ok(errorResponse);
                }

                //byte[] hash = Convert.FromBase64String(employee.Password);
                //byte[] salt = Convert.FromBase64String(employee.Salt);

                //bool success = VerifyHash(input.password, salt, hash);

                bool success = input.password==employee.Password?true:false;


                if (success)
                {
                    //密碼正確 發Token
                    JwtAuthUtil jwtAuthUtil = new JwtAuthUtil();
                    string jwtToken = jwtAuthUtil.GenerateToken_Employee(employee.Name, employee.Identity);

                    var response = new
                    {
                        statusCode = 200,
                        code = 0,
                        message = "登入成功",
                        data = new
                        {
                            identity = employee.Identity,
                            username = employee.Name,
                            token = jwtToken,
                        }
                    };

                    return Ok(response);
                }
                else
                {
                    //密碼錯誤
                    var errorResponse = new
                    {
                        statusCode = 400,
                        code = -1,
                        message = "帳號密碼錯誤"
                    };
                    return Ok(errorResponse);
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

        //EL-2
        [HttpPost]
        [JwtAuthFilter_E]
        [Route("api/employee/logout")]
        public IHttpActionResult PostEmployeeLogout()
        {
            try
            {
                JwtAuthUtil jwtAuthUtil = new JwtAuthUtil();
                string jwtToken = jwtAuthUtil.RevokeToken();
                var response = new
                {
                    statusCode = 200,
                    code = 0,
                    message = "登出成功",
                    data = new
                    {
                        
                        token = jwtToken,
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

    }
}
