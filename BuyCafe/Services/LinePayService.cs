using BuyCafe.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BuyCafe.Services
{
    public class LinePayService
    {
        // 獲取Web.config內的資訊
        string id = ConfigurationManager.AppSettings["secretId"];
        string secretKey = ConfigurationManager.AppSettings["secretKey"];
        string url = ConfigurationManager.AppSettings["baseUrl"];

        // HttpClient : 用於發送請求與接收回應
        // 建立靜態的HttpClient實例，建立多個LinePayService成員就可以共享
        static HttpClient client;

        // 透過建構函數，每次建立LinePayService物件時，就會建立靜態HttpClient實例
        public LinePayService()
        {
            client = new HttpClient();
        }

        // 發送交易請求
        public async Task<LinePayResponseDto> ReservePaymentAsync(LinePayRequestDto request)
        {
            string requestUrl = "/v3/payments/request";
            string jsonRequestBody = JsonConvert.SerializeObject(request);
            string nonce = Guid.NewGuid().ToString();
            string signature = HmacSignature.GenerateHmacSignature(secretKey, requestUrl, jsonRequestBody, nonce);

            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, url + requestUrl)
            {
                Content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            };

            // Add headers
            httpRequest.Headers.Add("X-LINE-ChannelId", id);
            httpRequest.Headers.Add("X-LINE-Authorization-Nonce", nonce);
            httpRequest.Headers.Add("X-LINE-Authorization", signature);

            HttpResponseMessage response = await client.SendAsync(httpRequest);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // 將回傳結果反序列化為LinePayResponseDto物件
                LinePayResponseDto result = JsonConvert.DeserializeObject<LinePayResponseDto>(responseContent);
                return result;
            }
            else
            {
                throw new Exception($"LinePay API請求失敗 : {responseContent}");
            }
        }

        // 確認訂單
        public async Task<PaymentConfirmResponseDto> GetPaymentStatusAsync(ConfirmRequestDto confirm)
        {
            string confirmUrl = $"/v3/payments/{confirm.transactionId}/confirm";
            string nonce = Guid.NewGuid().ToString();
            string jsonConfirm = JsonConvert.SerializeObject(confirm);

            string signature = HmacSignature.GenerateHmacSignature(secretKey, confirmUrl, jsonConfirm, nonce);

            HttpRequestMessage requestConfirm = new HttpRequestMessage(HttpMethod.Post, url + confirmUrl)
            {
                Content = new StringContent(jsonConfirm, Encoding.UTF8, "application/json")
            };

            requestConfirm.Headers.Add("X-LINE-ChannelId", id);
            requestConfirm.Headers.Add("X-LINE-Authorization-Nonce", nonce);
            requestConfirm.Headers.Add("X-LINE-Authorization", signature);

            HttpResponseMessage responseConfirm = await client.SendAsync(requestConfirm);
            string responseContent = await responseConfirm.Content.ReadAsStringAsync();

            if (responseConfirm.IsSuccessStatusCode)
            {
                // 將回傳結果反序列化為PaymentConfirmResponseDto物件
                PaymentConfirmResponseDto result = JsonConvert.DeserializeObject<PaymentConfirmResponseDto>(responseContent);
                return result;
            }
            else
            {
                throw new Exception($"LinePay API 請求失敗 : {responseContent}");
            }
        }
    }
}