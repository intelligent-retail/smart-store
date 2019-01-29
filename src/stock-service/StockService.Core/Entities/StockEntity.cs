using System;

namespace StockService.Entities
{
    // TODO: スキーマを確認する
    public class StockEntity
    {
        public long Id { get; set; }

        public string DocumentId { get; set; }

        public long TransactionId { get; set; }

        public DateTime TransactionDate { get; set; }

        public string TransactionType { get; set; }

        public string LocationCode { get; set; }

        public string CompanyCode { get; set; }

        public string StoreCode { get; set; }

        public string TerminalCode { get; set; }

        public int LineNo { get; set; }

        public string ItemCode { get; set; }

        public int Quantity { get; set; }
    }
}
