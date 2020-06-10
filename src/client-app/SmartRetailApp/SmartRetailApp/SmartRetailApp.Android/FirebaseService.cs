using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Firebase.Messaging;
using SmartRetailApp.Models;
using System;
using System.Linq;
using WindowsAzure.Messaging;

namespace SmartRetailApp.Droid
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FirebaseService : FirebaseMessagingService
    {
        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            string action = "";

            if (message.Data.ContainsKey("action"))
            {
                action = message.Data["action"];
            }

            // convert the incoming message to a local notification
            SendLocalNotification(action);

            // send the incoming message directly to the MainPage
            SendMessageToMainPage(action);
        }

        public override void OnNewToken(string deviceToken)
        {
            // TODO: save token instance locally, or log if desired

            // デバイスIDを記憶する
            (App.Current as App).DeviceId = deviceToken;

            SendRegistrationToServer(deviceToken);
        }

        void SendLocalNotification(string body)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            intent.PutExtra("message", body);
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);

            var notificationBuilder = new NotificationCompat.Builder(this, Constant.NotificationChannelName)
                .SetContentTitle("SmartStore Message")
                .SetSmallIcon(Resource.Drawable.ic_launcher)
                .SetContentText(body)
                .SetAutoCancel(true)
                .SetShowWhen(false)
                .SetContentIntent(pendingIntent);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                notificationBuilder.SetChannelId(Constant.NotificationChannelName);
            }

            var notificationManager = NotificationManager.FromContext(this);
            notificationManager.Notify(0, notificationBuilder.Build());
        }

        void SendMessageToMainPage(string action)
        {
            (App.Current as App)?.DoActionAsync(action);
        }

        void SendRegistrationToServer(string token)
        {
            try
            {
                NotificationHub hub = new NotificationHub(Constant.NotificationHubName, Constant.ListenConnectionString, this);

                // register device with Azure Notification Hub using the token from FCM
                Registration registration = hub.Register(token, Constant.SubscriptionTags);

                // subscribe to the SubscriptionTags list with a simple template.
                string pnsHandle = registration.PNSHandle;
                TemplateRegistration templateReg = hub.RegisterTemplate(pnsHandle, "defaultTemplate", Constant.FCMTemplateBody, Constant.SubscriptionTags);
            }
            catch (Exception e)
            {
                Log.Error(Constant.DebugTag, $"Error registering device: {e.Message}");
            }
        }
    }
}