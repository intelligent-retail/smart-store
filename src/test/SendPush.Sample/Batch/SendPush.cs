using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoxManagementService.Utilities;
using ConsoleAppFramework;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Logging;
using Sharprompt;

namespace SendPush.Sample.Batch
{
    public class SendPush
    {
        private const string Description = "デバイスIDにプッシュ通知を送信する";
        private readonly ILogger<SendPush> _logger;

        public SendPush(ILogger<SendPush> logger)
        {
            _logger = logger;
        }

        [Command("sendpush", Description)]
        public async Task Run(string deviceIdList)
        {
            var connectionString = Settings.Instance.NotificaitonHubConnectionStrings;
            var hubName = Settings.Instance.HubName;

            var actionList = new[] {"update_cart", "receipt"};
            var action = Prompt.Select("Select Action", actionList, actionList.Length);

            var nhClient = NotificationHubClient.CreateClientFromConnectionString(connectionString, hubName);

            foreach (var deviceToken in deviceIdList.Split(','))
            {
                if (action == "update_cart")
                {
                    await NotificationUtility.PushClosedCartNotificationAsync(deviceToken);
                }
                if (action == "receipt")
                {
                    await NotificationUtility.PushClosedCartNotificationAsync(deviceToken);
                }
            }

        }
    }
}
