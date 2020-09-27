namespace SmartRetailApp.Models
{
    public class Constant
    {
        // Cart API の名前
        public const string CartsApiName = "CartsApiName";

        // APIのキー
        public const string ApiKey = "ApiKey";

        // AppCenter Android用のキー
        public const string AppCenterKeyAndroid = "AppCenterKeyAndroid";

        // Azure AD B2C のテナント名 例）"contoso123tenant"
        // ポータル → Azure AD B2C → 概要 → ドメイン名の .onmicrosoft.com を除いた部分
        public const string TenantName = "TenantName";

        // Azure AD B2C のクライアントID（アプリケーションID）例）"aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"
        // ポータル → Azure AD B2C → アプリケーション → アプリケーション ID
        public const string ClientId = "ClientId";

        // Azure AD B2C のユーザーフロー 例）"B2C_1_signupsignin"
        // ポータル → Azure AD B2C → ユーザーフロー
        public const string PolicySignin = "PolicySignin";

        // AppCenter iOS用のキー
        public const string AppCenterKeyiOS = "AppCenterKeyiOS";

        // キーチェーン（iOSのみ） 例）com.sample.contoso
        // iOS プロジェクト→Entitlement.plist→Keychain
        // （iOS プロジェクト→info.plist→バンドル識別子と同じ）
        public const string IosKeyChain = "IosKeyChain";

        static readonly string[] scopes = { "" };
        static readonly string tenantId = $"{TenantName}.onmicrosoft.com";
        static readonly string authorityBase = $"https://{TenantName}.b2clogin.com/tfp/{tenantId}/";
        public static string AuthoritySignin => $"{authorityBase}{PolicySignin}";
        public static string[] Scopes => scopes;

        // 接続文字列
        // Azure Portal → Notification Hub → Access Policies → DefaultListenShared AccessSignature
        // ※ Listen のみの接続文字列でないと動作しないので注意
        public static string ListenConnectionString { get; set; } = "{DefaultListenSharedAccessSignature}";

        /// <summary>
        /// Notification Hub のハブ名
        /// </summary>
        public static string NotificationHubName { get; set; } = "{HubName}";

        /// <summary>
        /// 登録するタグ
        /// </summary>
        public static string[] SubscriptionTags { get; set; } = { "default" };

        /// <summary>
        /// Android のテンプレート
        /// </summary>
        public static string FCMTemplateBody { get; set; } = "{\"data\":{\"message\":\"$(messageParam)\"}}";

        /// <summary>
        /// iOS のテンプレート
        /// </summary>
        public static string APNTemplateBody { get; set; } = "{\"aps\":{\"alert\":\"$(messageParam)\"}}";

        /// <summary>
        /// Notification channels are used on Android devices starting with "Oreo"
        /// </summary>
        public static string NotificationChannelName { get; set; } = "SmartStoreChannel";

        /// <summary>
        /// Tag used in log messages to easily filter the device log
        /// during development.
        /// </summary>
        public static string DebugTag { get; set; } = "SmartStore";
    }
}
