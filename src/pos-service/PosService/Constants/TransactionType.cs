namespace PosService.Constants
{
    /// <summary>
    /// 取引ログ種別
    /// </summary>
    public class TransactionType
    {
        /// <summary>通常売上登録</summary>
        public static readonly TransactionType NormalSales = new TransactionType(101, nameof(NormalSales));

        /// <summary>取引中止された売上登録</summary>
        public static readonly TransactionType CanceledSales = new TransactionType(102, nameof(CanceledSales));

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="typeNo">取引ログ種別</param>
        /// <param name="name">取引ログ名称</param>
        private TransactionType(int typeNo, string name)
        {
            this.TypeNo = typeNo;
            this.Name = name;
        }

        /// <summary>取引ログ種別</summary>
        public int TypeNo { get; }

        /// <summary>取引ログ名称</summary>
        public string Name { get; }
    }
}
