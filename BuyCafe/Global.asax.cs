using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebSocketSharp.Server;

namespace BuyCafe
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static WebSocketServer webSocketServer;
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // 從 web.config 讀取 WebSocket 伺服器地址
            string webSocketAddress = ConfigurationManager.AppSettings["WebSocketServerAddress"];

            // 1. 建立 WebSocket 伺服器，監聽 localhost:8080
            webSocketServer = new WebSocketServer(webSocketAddress);

            // 2. 新增 NotifyBehavior 到路徑 "/notify"
            webSocketServer.AddWebSocketService<NotifyBehavior>("/notify");

            // 3. 啟動 WebSocket 伺服器
            webSocketServer.Start();
            Console.WriteLine($"WebSocket server started at {webSocketAddress}");
        }
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            var response = context.Response;
            //allow-origin直接用* 代表網域全開，或是這邊是要設定看對接的人網域是多少
            response.AddHeader("Access-Control-Allow-Origin", "*");
            response.AddHeader("X-Frame-Options", "ALLOW-FROM *");

            if (context.Request.HttpMethod == "OPTIONS")
            {
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST, DELETE, PATCH, PUT");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
                response.AddHeader("Access-Control-Max-Age", "1000000");
                response.End();
            }
        }

        protected void Application_End()
        {
            // 當應用程式結束時停止 WebSocket 伺服器
            if (webSocketServer != null)
            {
                webSocketServer.Stop();
                Console.WriteLine("WebSocket server stopped.");
            }
        }
    }
}
