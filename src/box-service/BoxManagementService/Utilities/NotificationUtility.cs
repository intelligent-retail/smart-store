using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace BoxManagementService.Utilities
{
    /// <summary>
    /// アプリケーション（スマートフォン）への通知ユーティリティ
    /// </summary>
    public static class NotificationUtility
    {
        /// <summary>
        /// アプリケーションにカート取引の終了を通知します。
        /// </summary>
        /// <param name="deviceId">通知先のデバイスID</param>
        /// <returns>非同期タスク</returns>
        public static Task PushClosedCartNotificationAsync(string deviceId) => PushNotificationAsync(deviceId, "receipt");

        /// <summary>
        /// アプリケーションにカート取引の更新を通知します。
        /// </summary>
        /// <param name="deviceId">通知先のデバイスID</param>
        /// <returns>非同期タスク</returns>
        public static Task PushUpdatedCartNotificationAsync(string deviceId) => PushNotificationAsync(deviceId, "update_cart");

        /// <summary>
        /// アプリケーションにactionを通知します。
        /// </summary>
        /// <param name="deviceId">通知先のデバイスID</param>
        /// <param name="action">通知するアクション(カスタムデータ)</param>
        /// <returns>非同期タスク</returns>
        private static Task PushNotificationAsync(string deviceId, string action)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, Settings.Instance.NotificationUri)
            {
                Headers =
                {
                    { "Accept", "application/json" },
                    { Settings.Instance.NotificationApiKeyHeader, Settings.Instance.NotificationApiKey }
                },
                Content = CreateJsonContent(new
                {
                    notification_target = new
                    {
                        type = "devices_target",
                        devices = new[] { $"{deviceId}" }
                    },
                    notification_content = new
                    {
                        name = "UpdateCart",
                        title = "UpdateCart",
                        body = "UpdateCart",
                        custom_data = new
                        {
                            action
                        }
                    }
                }),
            };

            return StaticHttpClient.Instance.SendAsync(request);

            HttpContent CreateJsonContent<T>(T value) => new ObjectContent<T>(value, new JsonMediaTypeFormatter());
        }
    }
}
