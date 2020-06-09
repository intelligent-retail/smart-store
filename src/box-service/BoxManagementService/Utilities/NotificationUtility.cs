using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dynamitey;
using Microsoft.Azure.NotificationHubs;

namespace BoxManagementService.Utilities
{
    /// <summary>
    /// アプリケーション（スマートフォン）への通知ユーティリティ
    /// </summary>
    public class NotificationUtility
    {
        public string ConnectionString { get; set; }
        public string HubName { get; set; }

        public NotificationUtility()
        {
            this.ConnectionString= Settings.Instance.NotificaitonHubConnectionStrings;
            this.HubName = Settings.Instance.HubName;
        }

        /// <summary>
        /// アプリケーションにカート取引の終了を通知します。
        /// </summary>
        /// <param name="deviceId">通知先のデバイスID</param>
        /// <returns>非同期タスク</returns>
        public async Task PushClosedCartNotificationAsync(string deviceId) => PushNotificationAsync(deviceId, "receipt");

        /// <summary>
        /// アプリケーションにカート取引の更新を通知します。
        /// </summary>
        /// <param name="deviceId">通知先のデバイスID</param>
        /// <returns>非同期タスク</returns>
        public async Task PushUpdatedCartNotificationAsync(string deviceId) => PushNotificationAsync(deviceId, "update_cart");

        /// <summary>
        /// アプリケーションにactionを通知します。
        /// </summary>
        /// <param name="deviceId">通知先のデバイスID</param>
        /// <param name="action">通知するアクション(カスタムデータ)</param>
        /// <returns>非同期タスク</returns>
        private async Task PushNotificationAsync(string deviceId, string action)
        {
            var nhClient = NotificationHubClient.CreateClientFromConnectionString(this.ConnectionString, this.HubName);

            if (IsTokenAndroid(deviceId))
            {
                // AndroidのデバイスIDを使ってNotificationHubにインストールする
                var fcmInstallation = new Installation
                {
                    InstallationId = Guid.NewGuid().ToString(),
                    Platform = NotificationPlatform.Fcm,
                    PushChannel = deviceId,
                    PushChannelExpired = false,
                    Tags = new[] { "fcm" }
                };
                await nhClient.CreateOrUpdateInstallationAsync(fcmInstallation);

                // デバイスにプッシュ通知を送信する
                var fcmContent = $"{{\"data\":{{\"action\":\"{action}\"}}}}";
                var outcomeFcmByDeviceId = await nhClient.SendDirectNotificationAsync(new FcmNotification(fcmContent), deviceId);

            }

            if (IsTokeniOS(deviceId))
            {
                Console.WriteLine($"iOS device token={deviceId}");

                // iOSのデバイスIDを使ってNotificationHubにインストールする
                var apnsInstallation = new Installation
                {
                    InstallationId = Guid.NewGuid().ToString(),
                    Platform = NotificationPlatform.Apns,
                    PushChannel = deviceId,
                    PushChannelExpired = false,
                    Tags = new[] { "apns" }
                };
                await nhClient.CreateOrUpdateInstallationAsync(apnsInstallation);

                // デバイスにプッシュ通知を送信する
                var apnsContent = $"{{\"aps\":{{\"alert\":\"Notification Hub test notification from SDK sample\"}},\"action\":\"{action}\"}}";
                var outcomeApnsByDeviceId = await nhClient.SendDirectNotificationAsync(new AppleNotification(apnsContent), deviceId);
            }
        }
        /// <summary>
        /// デバイストークンがAndoirdかどうか
        /// ※ iOSでなければtrue
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        static bool IsTokenAndroid(string token)
        {
            return !IsTokeniOS(token);
        }
        /// <summary>
        /// デバイストークンがiOSかどうか
        /// </summary>
        /// <param name="token">8888AAAABBBBCCCC99998888DDDDEEEE4444333300001111FFFF555566667777</param>
        /// <returns></returns>
        static bool IsTokeniOS(string token)
        {
            return token.Length == 64 && Regex.IsMatch(token, @"[A-Z0-9]{64}");
        }
    }
}
