namespace PosService.Constants
{
    /// <summary>
    /// カウンター種別
    /// </summary>
    public class CounterType
    {
        /// <summary>レシート</summary>
        public static readonly CounterType Receipt = new CounterType("Receipt", nameof(Receipt));

        /// <summary>取引ログ</summary>
        public static readonly CounterType Transaction = new CounterType("Transaction", nameof(Transaction));

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="code">カウンター種別コード</param>
        /// <param name="name">カウンター種別名称</param>
        private CounterType(string code, string name)
        {
            this.Code = code;
            this.Name = name;
        }

        /// <summary>カウンター種別コード</summary>
        public string Code { get; }

        /// <summary>カウンター種別名称</summary>
        public string Name { get; }
    }
}
