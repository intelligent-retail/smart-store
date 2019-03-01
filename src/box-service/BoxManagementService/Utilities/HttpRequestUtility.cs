using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace BoxManagementService.Utilities
{
    public class HttpRequestUtility
    {
        public static async Task<T> GetRequestParameterAsync<T>(HttpRequest req)
            => JsonConvert.DeserializeObject<T>(await GetRequestBodyAsync(req));

        public static async Task<string> GetRequestBodyAsync(HttpRequest req)
        {
            using (var reader = new StreamReader(req.Body))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
