using Newtonsoft.Json;

namespace BoxManagementService.DataCotracts.Responses
{
    public class CreateCartResponse : BoxResponse
    {
        [JsonProperty(PropertyName = "cartId")]
        public string CartId { get; set; }
    }
}
