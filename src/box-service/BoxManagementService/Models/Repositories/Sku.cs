using Newtonsoft.Json;

namespace BoxManagementService.Models.Repositories
{
    public class Sku
    {
        /// <summary>
        /// CosmosDBで必要なID
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// 法人コード
        /// </summary>
        [JsonProperty(PropertyName = "companyCode")]
        public string CompanyCode { get; set; }

        /// <summary>
        /// 店舗コード
        /// </summary>
        [JsonProperty(PropertyName = "storeCode")]
        public string StoreCode { get; set; }

        /// <summary>
        /// SKUコード
        /// </summary>
        [JsonProperty(PropertyName = "skuCode")]
        public string SkuCode { get; set; }

        /// <summary>
        /// 商品コード
        /// </summary>
        [JsonProperty(PropertyName = "itemCode")]
        public string ItemCode { get; set; }
    }
}
