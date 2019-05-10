using Newtonsoft.Json;
using PosApiMock.Models;
using SmartRetailApp.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SmartRetailApp.Services
{
    public class CartApiService
    {
        private static HttpClient httpClient = new HttpClient();

        public enum CartAction
        {
            items,
            bill
        };

        /// <summary>
        /// 取引開始
        /// </summary>
        /// <param name="cartStart"></param>
        /// <returns></returns>
        public async Task<CartStartResult> CartStartAsync(CartStart cartStart)
        {
            var jsonText = JsonConvert.SerializeObject(cartStart);

            try
            {
                var content = new StringContent(jsonText, Encoding.UTF8, "application/json");
                content.Headers.Add("x-functions-key", Settings.Instance.ApiKey);

                var cartApi = Settings.Instance.MainUrl + Settings.Instance.CartsApiName;
                var response = await httpClient.PostAsync(cartApi, content);

                if (response.IsSuccessStatusCode)
                {
                    // 戻り値のJSONをDownloadItemに変換
                    return JsonConvert.DeserializeObject<CartStartResult>(await response.Content.ReadAsStringAsync());
                }

                // エラー
                return JsonConvert.DeserializeObject<CartStartResult>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return new CartStartResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// カートの状態を取得
        /// </summary>
        /// <param name="cartStart"></param>
        /// <returns></returns>
        public async Task<CartStatus> CartStatusAsync(string cartId, CartAction action)
        {
            try
            {
                var cartApi = Settings.Instance.MainUrl + Settings.Instance.CartsApiName;
                var url = $"{cartApi}/{cartId}/{action}";
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    requestMessage.Headers.Add("x-functions-key",Settings.Instance.ApiKey);
                    var response = await httpClient.SendAsync(requestMessage);

                    if (response.IsSuccessStatusCode)
                    {
                        // 戻り値のJSONをDownloadItemに変換
                        return JsonConvert.DeserializeObject<CartStatus>(await response.Content.ReadAsStringAsync());
                    }

                    return new CartStatus
                    {
                        IsSuccess = false,
                        ErrorMessage = response.RequestMessage.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                return new CartStatus
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
            return new CartStatus
            {
                IsSuccess = false,
                ErrorMessage = "APIを取得できない"
            };
        }
    }
}
