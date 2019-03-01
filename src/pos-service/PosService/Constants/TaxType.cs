namespace PosService.Constants
{
    /// <summary>
    /// 税種別
    /// </summary>
    public class TaxType
    {
        /// <summary>内税</summary>
        public static readonly TaxType IncludedTax = new TaxType("01", nameof(IncludedTax));

        /// <summary>外税</summary>
        public static readonly TaxType ExcludedTax = new TaxType("02", nameof(ExcludedTax));

        /// <summary>非課税</summary>
        public static readonly TaxType ExemptTax = new TaxType("03", nameof(ExemptTax));

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="code">税種別コード</param>
        /// <param name="name">税種別名称</param>
        private TaxType(string code, string name)
        {
            this.Code = code;
            this.Name = name;
        }

        /// <summary>税種別コード</summary>
        public string Code { get; }

        /// <summary>税種別名称</summary>
        public string Name { get; }
    }
}
