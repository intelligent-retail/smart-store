using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace SendPush.Sample.Models
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
        /// Notification Hubsの接続文字列
        /// </summary>
        public string NotificaitonHubConnectionStrings => _instance[nameof(NotificaitonHubConnectionStrings)];

        /// <summary>
        /// Notification Hubsのハブ名
        /// </summary>
        public string NotificationHubName => _instance[nameof(NotificationHubName)];
    }
}
