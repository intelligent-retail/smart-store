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


        // set your tenant name, for example "contoso123tenant"
        static readonly string tenantName = "{Your Tenant Name}";

        // set your tenant id, for example: "contoso123tenant.onmicrosoft.com"
        static readonly string tenantId = "{Your Tenant Name}.onmicrosoft.com";

        // set your client/application id, for example: aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"
        static readonly string clientId = "{Your Client Id}";

        // set your sign up/in policy name, for example: "B2C_1_signupsignin"
        static readonly string policySignin = "{Your Policy Name}";

        // set to a unique value for your app, such as your bundle identifier. Used on iOS to share keychain access.
        static readonly string iosKeychainSecurityGroup = "{Your Bundle Identifier}";

        static readonly string[] scopes = { "" };
        static readonly string authorityBase = $"https://{tenantName}.b2clogin.com/tfp/{tenantId}/";
        public static string ClientId => clientId;
        public static string AuthoritySignin => $"{authorityBase}{policySignin}";
        public static string[] Scopes => scopes;
        public static string IosKeychainSecurityGroups => iosKeychainSecurityGroup;
    }
}
