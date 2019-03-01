using BoxManagementService.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BoxManagementService.Models.Repositories.Http
{
    /// <summary>
    /// HttpClientを利用するRepositoryクラスの基底クラス
    /// </summary>
    public abstract class HttpRepositoryBase
    {
        protected async Task<(RepositoryResult result, dynamic body)> GetAsync(HttpApi api, object query = null)
        {
            var response = await api.GetAsync(query);
            return await this.CreateResultAsync(response);
        }

        protected async Task<(RepositoryResult result, dynamic body)> PostAsync(HttpApi api, object query = null)
        {
            var response = await api.PostAsync(query);
            return await this.CreateResultAsync(response);
        }

        protected async Task<(RepositoryResult result, dynamic body)> PostAsync<T>(HttpApi api, T content, object query = null)
        {
            var response = await api.PostAsync(content, query);
            return await this.CreateResultAsync(response);
        }

        protected async Task<(RepositoryResult result, dynamic body)> PutAsync(HttpApi api, object query = null)
        {
            var response = await api.PutAsync(query);
            return await this.CreateResultAsync(response);
        }

        protected async Task<(RepositoryResult result, dynamic body)> PutAsync<T>(HttpApi api, T content, object query = null)
        {
            var response = await api.PutAsync(content, query);
            return await this.CreateResultAsync(response);
        }

        protected async Task<(RepositoryResult result, dynamic body)> DeleteAsync(HttpApi api, object query = null)
        {
            var response = await api.DeleteAsync(query);
            return await this.CreateResultAsync(response);
        }

        private async Task<(RepositoryResult result, dynamic body)> CreateResultAsync(HttpResponseMessage response)
        {
            var contentString = await response.Content.ReadAsStringAsync();

            var content = this.ParseJsonString(contentString);

            return response.IsSuccessStatusCode
                ? (RepositoryResult.OK, content)
                : (this.CreateResultErrorStatus(response, content == null ? contentString : (string)content["errorMessage"]), content);
        }

        private JObject ParseJsonString(string json)
        {
            try
            {
                return !string.IsNullOrWhiteSpace(json) ? JObject.Parse(json) : null;
            }
            catch
            {
                return null;
            }
        }

        private RepositoryResult CreateResultErrorStatus(HttpResponseMessage response, string errorMessage)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    return RepositoryResult.CreateInvalidParameter(errorMessage);
                case HttpStatusCode.NotFound:
                    return RepositoryResult.CreateNotFound(errorMessage);
                case HttpStatusCode.Conflict:
                    return RepositoryResult.CreateConflict(errorMessage);
                default:
                    throw new ApplicationException(errorMessage);
            }
        }
    }
}
