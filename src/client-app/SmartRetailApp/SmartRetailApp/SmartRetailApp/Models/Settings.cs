using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace SmartRetailApp.Models
{
    public class Settings
    {
        public static Settings Instance { get; } = new Settings();

        public string MainUrl => _configuration[$"{nameof(Settings)}:{nameof(MainUrl)}"];

        public string CartsApiName => _configuration[$"{nameof(Settings)}:{nameof(CartsApiName)}"];

        public string ApiKey => _configuration[$"{nameof(Settings)}:{nameof(ApiKey)}"];

        public string AppCenterKeyAndroid => _configuration[$"{nameof(Settings)}:{nameof(AppCenterKeyAndroid)}"];

        public string AppCenterKeyiOS => _configuration[$"{nameof(Settings)}:{nameof(AppCenterKeyiOS)}"];

        private IConfiguration _configuration;

        public Settings()
        {
            // アプリケーション設定の設定
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables();

            // アプリケーション設定の取得
            _configuration = configurationBuilder.Build();
        }
    }
}
