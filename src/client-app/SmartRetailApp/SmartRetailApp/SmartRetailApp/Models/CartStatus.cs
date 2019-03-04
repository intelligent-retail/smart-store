using Newtonsoft.Json;

namespace SmartRetailApp.Models
{
    /// <summary>
    /// カートの状態
    /// </summary>
    public class CartStatus
    {
        [JsonProperty(PropertyName = "cart")]
        public Cart Cart { get; set; }

        [JsonProperty(PropertyName = "errorCode ")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "ErrorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonProperty(PropertyName = "store")]
        public Store Store { get; set; }

        [JsonProperty(PropertyName = "user")]
        public User User { get; set; }
    }

    public class Cart
    {
        [JsonProperty(PropertyName = "balance")]
        public string Balance { get; set; }

        [JsonProperty(PropertyName = "cartId")]
        public string CartId { get; set; }

        [JsonProperty(PropertyName = "changeAmount")]
        public string ChangeAmount { get; set; }

        [JsonProperty(PropertyName = "depositAmount")]
        public string DepositAmount { get; set; }

        [JsonProperty(PropertyName = "receiptNo")]
        public int ReceiptNo { get; set; }

        [JsonProperty(PropertyName = "receiptText")]
        public object ReceiptText { get; set; }

        [JsonProperty(PropertyName = "subtotalAmount")]
        public float SubtotalAmount { get; set; }

        [JsonProperty(PropertyName = "totalAmount")]
        public float TotalAmount { get; set; }

        [JsonProperty(PropertyName = "totalQuantities")]
        public int TotalQuantities { get; set; }

        [JsonProperty(PropertyName = "transactionNo")]
        public int TransactionNo { get; set; }

        [JsonProperty(PropertyName = "cartState")]
        public string CartState { get; set; }

        [JsonProperty(PropertyName = "lineItems")]
        public Lineitem[] LineItems { get; set; }

        [JsonProperty(PropertyName = "payments")]
        public Payment[] Payments { get; set; }

        [JsonProperty(PropertyName = "taxes")]
        public Tax[] Taxes { get; set; }
    }

    public class Lineitem
    {
        [JsonProperty(PropertyName = "amount")]
        public float Amount { get; set; }

        [JsonProperty(PropertyName = "itemCode")]
        public string ItemCode { get; set; }

        [JsonProperty(PropertyName = "itemName")]
        public string ItemName { get; set; }

        [JsonProperty(PropertyName = "lineNo")]
        public int LineNo { get; set; }

        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }

        [JsonProperty(PropertyName = "unitPrice")]
        public float UnitPrice { get; set; }

        [JsonProperty(PropertyName = "imageUrls")]
        public string[] ImageUrls { get; set; }
    }

    public class Payment
    {
        [JsonProperty(PropertyName = "paymentAmount")]
        public string PaymentAmount { get; set; }

        [JsonProperty(PropertyName = "paymentCode")]
        public string PaymentCode { get; set; }

        [JsonProperty(PropertyName = "paymentName")]
        public string PaymentName { get; set; }

        [JsonProperty(PropertyName = "paymentNo")]
        public int PaymentNo { get; set; }
    }

    public class Tax
    {
        [JsonProperty(PropertyName = "taxAmount")]
        public float TaxAmount { get; set; }

        [JsonProperty(PropertyName = "taxName")]
        public string TaxName { get; set; }

        [JsonProperty(PropertyName = "taxNo")]
        public int TaxNo { get; set; }
    }

    public class Store
    {
        [JsonProperty(PropertyName = "storeCode")]
        public string StoreCode { get; set; }

        [JsonProperty(PropertyName = "storeName")]
        public string StoreName { get; set; }

        [JsonProperty(PropertyName = "terinalNo")]
        public int TerinalNo { get; set; }
    }

    public class User
    {
        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }
    }
}
