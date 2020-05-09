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

        // Azure AD B2C のテナント名 例）"contoso123tenant"
        // ポータル → Azure AD B2C → 概要 → ドメイン名の .onmicrosoft.com を除いた部分
        static readonly string tenantName = "Your_Tenant_Name";

        // Azure AD B2C のクライアントID（アプリケーションID）例）"aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"
        // ポータル → Azure AD B2C → アプリケーション → アプリケーション ID
        static readonly string clientId = "Your_Client_Id";

        // Azure AD B2C のユーザーフロー 例）"B2C_1_signupsignin"
        // ポータル → Azure AD B2C → ユーザーフロー
        static readonly string policySignin = "Your_Policy_Name";

        // バンドル識別子（iOSのみ） 例）com.sample.contoso
        // iOS プロジェクト→プロパティ→バンドル識別子
        static readonly string iosKeychainSecurityGroup = "Your_Bundle_Identifier";

        static readonly string[] scopes = { "" };
        static readonly string tenantId = $"{tenantName}.onmicrosoft.com";
        static readonly string authorityBase = $"https://{tenantName}.b2clogin.com/tfp/{tenantId}/";
        public static string ClientId => clientId;
        public static string AuthoritySignin => $"{authorityBase}{policySignin}";
        public static string[] Scopes => scopes;
        public static string IosKeychainSecurityGroups => iosKeychainSecurityGroup;
    }
}
