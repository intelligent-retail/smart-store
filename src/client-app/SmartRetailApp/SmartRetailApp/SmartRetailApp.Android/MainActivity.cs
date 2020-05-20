using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using ZXing.Mobile;
using Android.Gms.Common;
using Android.Util;
using SmartRetailApp.Models;

namespace SmartRetailApp.Droid
{
    [Activity(Label = "SmartRetailApp", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            ZXing.Net.Mobile.Forms.Android.Platform.Init();
            MobileBarcodeScanner.Initialize(Application);

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            if (IsPlayServiceAvailable() == false)
            {
                throw new Exception("This device does not have Google Play Services and cannot receive push notifications.");
            }

            CreateNotificationChannel();
        }

        protected override void OnNewIntent(Intent intent)
        {
            if (intent.Extras != null)
            {
                var message = intent.GetStringExtra("message");
                (App.Current as App)?.PushCartPageAsync();
            }

            base.OnNewIntent(intent);
        }

        bool IsPlayServiceAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                    Log.Debug(Constant.DebugTag, GoogleApiAvailability.Instance.GetErrorString(resultCode));
                else
                {
                    Log.Debug(Constant.DebugTag, "This device is not supported");
                }
                return false;
            }
            return true;
        }

        void CreateNotificationChannel()
        {
            // Notification channels are new as of "Oreo".
            // There is no need to create a notification channel on older versions of Android.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelName = Constant.NotificationChannelName;
                var channelDescription = String.Empty;
                var channel = new NotificationChannel(channelName, channelName, NotificationImportance.Default)
                {
                    Description = channelDescription
                };

                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }
        }
    }
}