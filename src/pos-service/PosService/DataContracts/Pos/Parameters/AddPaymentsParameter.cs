using System.Collections.Generic;
using Newtonsoft.Json;

namespace PosService.DataContracts.Pos.Parameters
{
    /// <summary>
    /// 支払追加パラメーター
    /// </summary>
    public class AddPaymentsParameter : ParameterBase
    {
        /// <summary>追加する支払情報</summary>
        [JsonProperty(PropertyName = "payments", Required = Required.Always)]
        public List<AddPaymentsDetail> Payments { get; set; }

        /// <summary>
        /// 支払追加詳細
        /// </summary>
        public class AddPaymentsDetail
        {
            /// <summary>支払コード</summary>
            [JsonProperty(PropertyName = "paymentCode", Required = Required.Always)]
            public string PaymentCode { get; set; }

            /// <summary>支払金額</summary>
            [JsonProperty(PropertyName = "amount", Required = Required.Always)]
            public decimal Amount { get; set; }
        }
    }
}
