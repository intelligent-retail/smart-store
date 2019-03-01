namespace PosService.Constants
{
    /// <summary>
    /// 在庫引当種別
    /// </summary>
    public class StockAllocationType
    {
        /// <summary>仮引当</summary>
        public static readonly StockAllocationType ProvisionalAllocation = new StockAllocationType("01", nameof(ProvisionalAllocation));

        /// <summary>引当</summary>
        public static readonly StockAllocationType Allocation = new StockAllocationType("02", nameof(Allocation));

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="code">在庫引当コード</param>
        /// <param name="name">在庫引当名称</param>
        private StockAllocationType(string code, string name)
        {
            this.Code = code;
            this.Name = name;
        }

        /// <summary>在庫引当コード</summary>
        public string Code { get; }

        /// <summary>在庫引当名称</summary>
        public string Name { get; }
    }
}
