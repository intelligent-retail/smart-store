using Newtonsoft.Json;
using System;

namespace BoxManagementService.Models.Repositories
{
    public class BoxState
    {
        /// <summary>
        /// CosmosDBで必要なID
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// BOX識別文字列
        /// </summary>
        [JsonProperty(PropertyName = "boxId")]
        public string BoxId { get; set; }

        /// <summary>
        /// Device識別文字列（スマートフォンのデバイスID）
        /// </summary>
        [JsonProperty(PropertyName = "deviceId")]
        public string DeviceId { get; set; }

        /// <summary>
        /// ユーザー識別文字列
        /// </summary>
        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        /// <summary>
        /// カート識別文字列
        /// </summary>
        [JsonProperty(PropertyName = "cartId")]
        public string CartId { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        [JsonProperty(PropertyName = "updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
