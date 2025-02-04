using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSocketSharp.Server;
using WebSocketSharp;

namespace BuyCafe
{
    public class NotifyBehavior : WebSocketBehavior
    {
        private static List<NotifyBehavior> clients = new List<NotifyBehavior>();

        protected override void OnOpen()
        {
            clients.Add(this);
            Send(JsonConvert.SerializeObject(new { message = "Connection Opened" }));
        }

        protected override void OnClose(CloseEventArgs e)
        {
            clients.Remove(this);
        }

        // 廣播訂單更新訊息給所有連接的客戶端
        public static void BroadcastOrderUpdate(object message)
        {
            var orderUpdateMessage = JsonConvert.SerializeObject(message);

            // 發送給所有連接的客戶端
            foreach (var client in clients)
            {
                client.Send(orderUpdateMessage);
            }
        }
    }
}