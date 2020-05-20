namespace SmartRetailApp.Models
{
    public class Constant
    {
        // Cart API の名前
        public const string CartsApiName = "your_CartsApiName";

        // APIのキー
        public const string ApiKey = "your_ApiKey";


        // AppCenter Android用のキー
        public const string AppCenterKeyAndroid = "your_AppCenterKeyAndroid";

        // AppCenter iOS用のキー
        public const string AppCenterKeyiOS = "your_AppCenterKeyiOS";

        // Azure Portal の Notification Hub Namespace の名前
        public static string NotificationHubName { get; set; } = "YourNotificationHubName";

        // 接続文字列
        // Azure Portal → Notification Hub → Access Policies → DefaultListenShared AccessSignature
        // ※ Listen のみの接続文字列でないと動作しないので注意
        public static string ListenConnectionString { get; set; } = "YourListenConnectionString";

        // 登録するタグ
        public static string[] SubscriptionTags { get; set; } = { "default", "yoga" };

        public static string APNTemplateBody { get; set; } = "{\"aps\":{\"alert\":\"$(messageParam)\"}}";
    }
}
