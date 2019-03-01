using System.Net.Http;

namespace PosService.Utilities
{
    /// <summary>
    /// すべての Azure Functions 関数の実行において共通的に使用する <see cref="HttpClient"/> です。
    /// </summary>
    internal static class StaticHttpClient
    {
        /// <summary>
        /// <see cref="HttpClient"/> クラスの唯一のインスタンスを取得します。
        /// </summary>
        public static HttpClient Instance { get; } = new HttpClient();
    }
}
