﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dynamitey;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Logging;

namespace BoxManagementService.Utilities
{
    /// <summary>
    /// アプリケーション（スマートフォン）への通知ユーティリティ
    /// </summary>
    public class NotificationUtility
    {
        private readonly ILogger _logger;
        public string ConnectionString { get; set; }
        public string HubName { get; set; }

        public NotificationUtility(ILogger logger)
        {
            _logger = logger;
            this.ConnectionString = Settings.Instance.NotificaitonHubConnectionStrings;
            this.HubName = Settings.Instance.NotificationHubName;

        }

        /// <summary>
        /// アプリケーションにカート取引の終了を通知します。
        /// </summary>
        /// <param name="deviceId">通知先のデバイスID</param>
        /// <returns>非同期タスク</returns>
        public async Task PushClosedCartNotificationAsync(string deviceId) => await PushNotificationAsync(deviceId, "receipt");

        /// <summary>
        /// アプリケーションにカート取引の更新を通知します。
        /// </summary>
        /// <param name="deviceId">通知先のデバイスID</param>
        /// <returns>非同期タスク</returns>
        public async Task PushUpdatedCartNotificationAsync(string deviceId) => await PushNotificationAsync(deviceId, "update_cart");

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
                _logger.LogInformation($"Android device token={deviceId}");

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

                // 戻り値を使ってログを出力する
                await GetPushDetailsAndPrintOutcome("FCM Direct", nhClient, outcomeFcmByDeviceId);
            }

            if (IsTokeniOS(deviceId))
            {
                _logger.LogInformation($"iOS device token={deviceId}");

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

                // 戻り値を使ってログを出力する
                await GetPushDetailsAndPrintOutcome("APNS Direct", nhClient, outcomeApnsByDeviceId);
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

        private async Task GetPushDetailsAndPrintOutcome(
            string pnsType,
            NotificationHubClient nhClient,
            NotificationOutcome notificationOutcome)
        {
            // The Notification ID is only available for Standard SKUs. For Basic and Free SKUs the API to get notification outcome details can not be called.
            if (string.IsNullOrEmpty(notificationOutcome.NotificationId))
            {
                PrintPushNoOutcome(pnsType);
                return;
            }

            var details = await WaitForThePushStatusAsync(pnsType, nhClient, notificationOutcome);
            NotificationOutcomeCollection collection = null;
            switch (pnsType)
            {
                case "FCM":
                case "FCM Silent":
                case "FCM Tags":
                case "FCM Direct":
                    collection = details.FcmOutcomeCounts;
                    break;

                case "APNS":
                case "APNS Silent":
                case "APNS Tags":
                case "APNS Direct":
                    collection = details.ApnsOutcomeCounts;
                    break;

                case "WNS":
                    collection = details.WnsOutcomeCounts;
                    break;
                default:
                    _logger.LogInformation("Invalid Sendtype");
                    break;
            }

            PrintPushOutcome(pnsType, details, collection);
        }

        private void PrintPushOutcome(string pnsType, NotificationDetails details, NotificationOutcomeCollection collection)
        {
            if (collection != null)
            {
                _logger.LogInformation($"{pnsType} outcome: " + string.Join(",", collection.Select(kv => $"{kv.Key}:{kv.Value}")));
            }
            else
            {
                _logger.LogInformation($"{pnsType} no outcomes.");
            }
            _logger.LogInformation($"{pnsType} error details URL: {details.PnsErrorDetailsUri}");
        }

        private void PrintPushNoOutcome(string pnsType)
        {
            _logger.LogInformation($"{pnsType} has no outcome due to it is only available for Standard SKU pricing tier.");
        }
        private async Task<NotificationDetails> WaitForThePushStatusAsync(string pnsType, NotificationHubClient nhClient, NotificationOutcome notificationOutcome)
        {
            var notificationId = notificationOutcome.NotificationId;
            var state = NotificationOutcomeState.Enqueued;
            var count = 0;
            NotificationDetails outcomeDetails = null;
            while ((state == NotificationOutcomeState.Enqueued || state == NotificationOutcomeState.Processing) && ++count < 10)
            {
                try
                {
                    _logger.LogInformation($"{pnsType} status: {state}");
                    outcomeDetails = await nhClient.GetNotificationOutcomeDetailsAsync(notificationId);
                    state = outcomeDetails.State;
                }
                catch (MessagingEntityNotFoundException)
                {
                    // It's possible for the notification to not yet be enqueued, so we may have to swallow an exception
                    // until it's ready to give us a new state.
                }
                Thread.Sleep(1000);
            }
            return outcomeDetails;
        }
    }
}
