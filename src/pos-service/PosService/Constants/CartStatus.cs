namespace PosService.Constants
{
    /// <summary>
    /// カート状態
    /// </summary>
    public class CartStatus
    {
        /// <summary>商品登録</summary>
        public static readonly CartStatus EnteringItem = new CartStatus("01", nameof(EnteringItem));

        /// <summary>小計</summary>
        public static readonly CartStatus Paying = new CartStatus("02", nameof(Paying));

        /// <summary>取引完了</summary>
        public static readonly CartStatus Completed = new CartStatus("03", nameof(Completed));

        /// <summary>取引中止</summary>
        public static readonly CartStatus Canceled = new CartStatus("04", nameof(Canceled));

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="code">状態コード</param>
        /// <param name="name">状態名称</param>
        private CartStatus(string code, string name)
        {
            this.Code = code;
            this.Name = name;
        }

        /// <summary>状態コード</summary>
        public string Code { get; }

        /// <summary>状態名称</summary>
        public string Name { get; }
    }
}
