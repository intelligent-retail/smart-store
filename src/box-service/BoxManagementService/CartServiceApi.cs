using System;
using System.Net.Http;
using BoxManagementService.Http;
using BoxManagementService.Utilities;

namespace BoxManagementService
{
    public class CartServiceApi
    {
        /// <summary>
        /// POSのカート一覧URIを取得します。
        /// </summary>
        /// <returns>URI</returns>
        public static Uri CartsBaseUri => new Uri(ToDirectory(Settings.Instance.PosCartsUri));

        /// <summary>
        /// 仮想ディレクトリに/がついていなければ付けます。
        /// 引数の仮想ディレクトリは最終文字だけを参照するので、絶対URLでもディレクトリ名だけでも処理可能です。
        /// </summary>
        /// <param name="virtualDirectory">仮想ディレクトリ</param>
        /// <returns>仮想ディレクトリ文字列</returns>
        public static string ToDirectory(string virtualDirectory) => virtualDirectory + (virtualDirectory.EndsWith('/') ? "" : "/");

        private HttpClient _client;

        public CartServiceApi(HttpClient client)
        {
            this._client = client;
        }

        #region HttpAPI生成

        /// <summary>
        /// POSのカート一覧APIを取得します。
        /// </summary>
        /// <returns>API</returns>
        public HttpApi CartsApi => HttpApi.Create(this._client, this.CartsUri, Settings.Instance.PosApiKeyHeader, Settings.Instance.PosApiKey);

        /// <summary>
        /// POSのカートAPIを取得します。
        /// </summary>
        /// <param name="cartId">カート識別文字列</param>
        /// <returns>API</returns>
        public HttpApi GetCartApi(string cartId) => HttpApi.Create(this._client, this.GetCartUri(cartId), Settings.Instance.PosApiKeyHeader, Settings.Instance.PosApiKey);

        /// <summary>
        /// POSのカート商品一覧APIを取得します。
        /// </summary>
        /// <param name="cartId">カート識別文字列</param>
        /// <returns>API</returns>
        public HttpApi GetCartItemsApi(string cartId) => HttpApi.Create(this._client, this.GetCartItemsUri(cartId), Settings.Instance.PosApiKeyHeader, Settings.Instance.PosApiKey);

        /// <summary>
        /// POSのカート商品単品のAPIを取得します。
        /// </summary>
        /// <param name="cartId">カート識別文字列</param>
        /// <param name="itemCode">商品コード</param>
        /// <returns>API</returns>
        public HttpApi GetCartItemApi(string cartId, string itemCode) => HttpApi.Create(this._client, this.GetCartItemUri(cartId, itemCode), Settings.Instance.PosApiKeyHeader, Settings.Instance.PosApiKey);

        /// <summary>
        /// POSのカート小計APIを取得します。
        /// </summary>
        /// <param name="cartId">カート識別文字列</param>
        /// <returns>API</returns>
        public HttpApi GetCartSubtotalApi(string cartId) => HttpApi.Create(this._client, this.GetCartSubtotalUri(cartId), Settings.Instance.PosApiKeyHeader, Settings.Instance.PosApiKey);

        /// <summary>
        /// POSのカート支払APIを取得します。
        /// </summary>
        /// <param name="cartId">カート識別文字列</param>
        /// <returns>API</returns>
        public HttpApi GetCartPaymentsApi(string cartId) => HttpApi.Create(this._client, this.GetCartPaymentsUri(cartId), Settings.Instance.PosApiKeyHeader, Settings.Instance.PosApiKey);

        /// <summary>
        /// POSのカート精算APIを取得します。
        /// </summary>
        /// <param name="cartId">カート識別文字列</param>
        /// <returns>API</returns>
        public HttpApi GetCartBillApi(string cartId) => HttpApi.Create(this._client, this.GetCartBillUri(cartId), Settings.Instance.PosApiKeyHeader, Settings.Instance.PosApiKey);

        #endregion

        #region URI生成

        /// <summary>
        /// POSのカート一覧URIを取得します。
        /// </summary>
        /// <returns>URI</returns>
        private Uri CartsUri => CartsBaseUri;

        /// <summary>
        /// POSのカートURIを取得します。
        /// </summary>
        /// <param name="cartId">カート識別文字列</param>
        /// <returns>URI</returns>
        private Uri GetCartUri(string cartId) => new Uri(CartsBaseUri, ToDirectory(cartId));

        /// <summary>
        /// POSのカート商品一覧URIを取得します。
        /// </summary>
        /// <param name="cartId">カート識別文字列</param>
        /// <returns>URI</returns>
        private Uri GetCartItemsUri(string cartId) => new Uri(this.GetCartUri(cartId), ToDirectory(Settings.Instance.PosCartItemsPath));

        /// <summary>
        /// POSのカート商品単品のURIを取得します。
        /// </summary>
        /// <param name="cartId">カート識別文字列</param>
        /// <param name="itemCode">商品コード</param>
        /// <returns>URI</returns>
        private Uri GetCartItemUri(string cartId, string itemCode) => new Uri(this.GetCartItemsUri(cartId), itemCode);

        /// <summary>
        /// POSのカート小計URIを取得します。
        /// </summary>
        /// <param name="cartId">カート識別文字列</param>
        /// <returns>URI</returns>
        private Uri GetCartSubtotalUri(string cartId) => new Uri(this.GetCartUri(cartId), ToDirectory(Settings.Instance.PosCartSubtotalPath));

        /// <summary>
        /// POSのカート支払URIを取得します。
        /// </summary>
        /// <param name="cartId">カート識別文字列</param>
        /// <returns>URI</returns>
        private Uri GetCartPaymentsUri(string cartId) => new Uri(this.GetCartUri(cartId), ToDirectory(Settings.Instance.PosCartPaymentsPath));

        /// <summary>
        /// POSのカート精算URIを取得します。
        /// </summary>
        /// <param name="cartId">カート識別文字列</param>
        /// <returns>URI</returns>
        private Uri GetCartBillUri(string cartId) => new Uri(this.GetCartUri(cartId), ToDirectory(Settings.Instance.PosCartBillPath));

        #endregion
    }
}
