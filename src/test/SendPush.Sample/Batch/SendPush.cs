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
    public class SendPush : ConsoleAppBase
    {
        private const string Description = "デバイスIDにプッシュ通知を送信する";
        private const string update_cart = nameof(update_cart);
        private const string receipt = nameof(receipt);
        private readonly ILogger<SendPush> _logger;

        public SendPush(ILogger<SendPush> logger)
        {
            _logger = logger;
        }

        [Command("sendpush", Description)]
        public async Task Run(string deviceIdList)
        {
            var connectionString = Models.Settings.Instance.NotificaitonHubConnectionStrings;
            var hubName = Models.Settings.Instance.HubName;

            var actionList = new[] { update_cart, receipt };
            var action = Prompt.Select("Select Action", actionList);

            var utiility = new NotificationUtility();
            utiility.ConnectionString = connectionString;
            utiility.HubName = hubName;

            foreach (var deviceToken in deviceIdList.Split(','))
            {
                if (action == update_cart)
                {
                    await utiility.PushUpdatedCartNotificationAsync(deviceToken);
                }
                if (action == receipt)
                {
                    await utiility.PushClosedCartNotificationAsync(deviceToken);
                }
            }

        }
    }
}
