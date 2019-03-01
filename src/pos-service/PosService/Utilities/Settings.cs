using Microsoft.Extensions.Configuration;

namespace PosService.Utilities
{
    /// <summary>
    /// 設定値管理クラス
    /// </summary>
    public class Settings
    {
        /// <summary>コンフィグルート</summary>
        private readonly IConfigurationRoot _instance;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private Settings()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", true)
                .AddEnvironmentVariables();

            this._instance = builder.Build();
        }

        /// <summary>インスタンス</summary>
        public static Settings Instance { get; } = new Settings();

        /// <summary>アプリケーションタイムゾーン</summary>
        public string ApplicationTimeZone => this._instance[nameof(this.ApplicationTimeZone)];

        /// <summary>レシート幅</summary>
        public int ReceiptWith => int.Parse(this._instance[nameof(this.ReceiptWith)]);

        /// <summary>Functions 実行時の API ヘッダ名称</summary>
        public string FunctionsApiKeyHeader => this._instance[nameof(this.FunctionsApiKeyHeader)];

        /// <summary>商品マスターサービスの URL</summary>
        public string ItemMasterUri => this._instance[nameof(this.ItemMasterUri)];

        /// <summary>商品マスターサービスの API キー</summary>
        public string ItemMasterApiKey => this._instance[nameof(this.ItemMasterApiKey)];

        /// <summary>在庫管理サービスの URL</summary>
        public string StockUri => this._instance[nameof(this.StockUri)];

        /// <summary>在庫管理サービスの API キー</summary>
        public string StockApiKey => this._instance[nameof(this.StockApiKey)];
    }
}
