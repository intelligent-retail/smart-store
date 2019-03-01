using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace BoxManagementService.Http
{
    public class HttpApi
    {
        public Uri Uri { get; }
        public HeaderApiKey ApiKey { get; }

        private readonly HttpClient _client;

        public static HttpApi Create(HttpClient client, Uri uri) => new HttpApi(client, uri);

        public static HttpApi Create(HttpClient client, Uri uri, string header, string key) => new HttpApi(client, uri, new HeaderApiKey(header, key));

        public HttpApi(HttpClient client, Uri uri)
        {
            this._client = client;
            this.Uri = uri;
            this.ApiKey = null;
        }

        public HttpApi(HttpClient client, Uri uri, HeaderApiKey key)
        {
            this._client = client;
            this.Uri = uri;
            this.ApiKey = key;
        }

        public bool HasApiKey => this.ApiKey?.HasKey ?? false;

        public Task<HttpResponseMessage> GetAsync(object query = null)
            => this.SendAsync(HttpMethod.Get, this, query);

        public Task<HttpResponseMessage> PostAsync(object query = null)
            => this.SendAsync(HttpMethod.Post, this, query);

        public Task<HttpResponseMessage> PostAsync<T>(T content, object query = null)
            => this.SendAsync(HttpMethod.Post, this, content, query);

        public Task<HttpResponseMessage> PutAsync(object query = null)
            => this.SendAsync(HttpMethod.Put, this, query);

        public Task<HttpResponseMessage> PutAsync<T>(T content, object query = null)
            => this.SendAsync(HttpMethod.Put, this, content, query);

        public Task<HttpResponseMessage> DeleteAsync(object query = null)
            => this.SendAsync(HttpMethod.Delete, this, query);

        private Task<HttpResponseMessage> SendAsync(HttpMethod method, HttpApi api, object query = null)
            => this.SendAsync(method, api, (object)null, query);

        private Task<HttpResponseMessage> SendAsync<T>(HttpMethod method, HttpApi api, T content, object query = null)
        {
            var request = new HttpRequestMessage(method, this.SetQuery(api.Uri, query));
            request.Headers.Add("Accept", "application/json");
            if (api.HasApiKey)
            {
                request.Headers.Add(api.ApiKey.Header, api.ApiKey.Key);
            }
            if (content != null)
            {
                request.Content = new ObjectContent<T>(content, new JsonMediaTypeFormatter());
            }

            return this._client.SendAsync(request);
        }

        private Uri SetQuery(Uri uri, object query)
        {
            if (query == null)
            {
                return uri;
            }

            var builder = new UriBuilder(uri)
            {
                Query = string.Join("&", query.GetType().GetProperties().Select(p => $"{p.Name}={p.GetValue(query)}"))
            };
            return builder.Uri;
        }

        public class HeaderApiKey
        {
            public string Header { get; }
            public string Key { get; }

            public HeaderApiKey(string header, string key)
            {
                this.Header = header;
                this.Key = key;
            }

            public bool HasKey => !string.IsNullOrWhiteSpace(this.Header) && !string.IsNullOrWhiteSpace(this.Key);
        }
    }
}
