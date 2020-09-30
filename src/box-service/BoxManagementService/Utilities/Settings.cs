using Microsoft.Extensions.Configuration;

namespace BoxManagementService.Utilities
{
    public class Settings
    {
        public Settings()
        {
            var builder = new ConfigurationBuilder()
                            .AddJsonFile("local.settings.json", true)
                            .AddEnvironmentVariables();
            _instance = builder.Build();
        }

        public static Settings Instance { get; } = new Settings();

        private readonly IConfigurationRoot _instance;

        /// <summary>
        /// IoT Hub接続文字列
        /// </summary>
        public string IotHubConnectionString => _instance[nameof(IotHubConnectionString)];

        /// <summary>
        /// POSサービスのAPIキーヘッダ
        /// </summary>
        public string PosApiKeyHeader => _instance[nameof(PosApiKeyHeader)];

        /// <summary>
        /// POSサービスのAPIキー
        /// </summary>
        public string PosApiKey => _instance[nameof(PosApiKey)];

        /// <summary>
        /// POSのカート基底URI
        /// </summary>
        public string PosCartsUri => _instance[nameof(PosCartsUri)];

        /// <summary>
        /// POSのカート商品パス
        /// </summary>
        public string PosCartItemsPath => _instance[nameof(PosCartItemsPath)];

        /// <summary>
        /// POSのカート小計パス
        /// </summary>
        public string PosCartSubtotalPath => _instance[nameof(PosCartSubtotalPath)];

        /// <summary>
        /// POSのカート支払パス
        /// </summary>
        public string PosCartPaymentsPath => _instance[nameof(PosCartPaymentsPath)];

        /// <summary>
        /// POSのカート精算パス
        /// </summary>
        public string PosCartBillPath => _instance[nameof(PosCartBillPath)];

        /// <summary>
        /// Notification Hubsの接続文字列
        /// </summary>
        public string NotificaitonHubConnectionStrings => _instance[nameof(NotificaitonHubConnectionStrings)];
        
        /// <summary>
        /// Notification Hubsのハブ名
        /// </summary>
        public string HubName => _instance[nameof(HubName)];
    }
}
