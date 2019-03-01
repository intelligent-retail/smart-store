using Newtonsoft.Json;

namespace PosService.Models
{
    /// <summary>
    /// ユーザー情報
    /// </summary>
    public class UserInfo
    {
        /// <summary>ユーザー ID</summary>
        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        /// <summary>ユーザー名</summary>
        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }
    }
}
