using Newtonsoft.Json;

namespace PosService.DataContracts.Pos.Responses
{
    /// <summary>
    /// カート作成結果
    /// </summary>
    public class CreateCartResponse : ResponseBase
    {
        /// <summary>カート ID</summary>
        [JsonProperty(PropertyName = "cartId")]
        public string CartId { get; set; }
    }
}
