using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace PosService.Utilities
{
    /// <summary>
    /// HTTP リクエストユーティリティ
    /// </summary>
    public static class HttpRequestUtility
    {
        /// <summary>
        /// HTTP リクエストのパラメーターを取得します
        /// </summary>
        /// <typeparam name="T">取得するパラメーターの型</typeparam>
        /// <param name="req">HTTP リクエスト</param>
        /// <returns>HTTP リクエストのパラメーター</returns>
        public static async Task<T> GetRequestParameterAsync<T>(HttpRequest req)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(await GetRequestBodyAsync(req));
            }
            catch (JsonSerializationException)
            {
                return default(T);
            }
        }

        /// <summary>
        /// HTTP リクエストのボディを取得します
        /// </summary>
        /// <param name="req">HTTP リクエスト</param>
        /// <returns>HTTP リクエストのボディ</returns>
        private static async Task<string> GetRequestBodyAsync(HttpRequest req)
        {
            using (var reader = new StreamReader(req.Body))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
