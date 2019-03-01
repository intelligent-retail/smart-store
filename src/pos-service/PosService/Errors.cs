namespace PosService
{
    /// <summary>
    /// エラー一覧
    /// </summary>
    public class Errors
    {
        //// HACK 必要に応じ、多言語対応できる作りにする

        /// <summary>カートを作成できませんでした。</summary>
        public static readonly string CannotCreateCart = "カートを作成できませんでした。";

        /// <summary>カートが見つかりませんでした。</summary>
        public static readonly string CartNotFound = "カートが見つかりませんでした。";

        /// <summary>商品が見つかりませんでした。</summary>
        public static readonly string ItemNotFound = "商品が見つかりませんでした。";

        /// <summary>これ以上、商品を削除できません。</summary>
        public static readonly string CannotDeleteItem = "これ以上、商品を削除できません。";

        /// <summary>処理を受け付けることができません。</summary>
        public static readonly string BadCartStatus = "処理を受け付けることができません。";

        /// <summary>パラメータに誤りがあります。</summary>
        public static readonly string ParameterError = "パラメータに誤りがあります。";

        /// <summary>在庫情報を更新できません。</summary>
        public static readonly string CannotUpdateStocks = "在庫情報を更新できません。";
    }
}
