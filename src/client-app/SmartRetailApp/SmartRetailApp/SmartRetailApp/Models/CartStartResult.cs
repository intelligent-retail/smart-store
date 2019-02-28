using Newtonsoft.Json;

namespace PosApiMock.Models
{

    public class CartStartResult
    {
        /// <summary>
        /// 理成功フラグ|処理成功時:true、処理失敗時:false
        /// </summary>
        [JsonProperty(PropertyName = "isSuccess")]
        public bool IsSuccess { get; set; }

        /// <summary>
        /// エラーコード
        /// </summary>
        [JsonProperty(PropertyName = "errorCode")]
        public string ErrorCode { get; set; }

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// カートID 6748D99C-5338-4209-B4CF-4300580F474D
        /// </summary>
        [JsonProperty(PropertyName = "cartId")]
        public string CartId { get; set; }
    }
}
