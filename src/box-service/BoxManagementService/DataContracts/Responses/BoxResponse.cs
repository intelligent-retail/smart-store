using Newtonsoft.Json;

namespace BoxManagementService.DataCotracts.Responses
{
    public class BoxResponse
    {
        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }
    }
}
