using BoxManagementService.DataCotracts.Responses;
using BoxManagementService.Models.Repositories;

namespace BoxManagementService
{
    /// <summary>
    /// BOX レスポンスを作成します。
    /// </summary>
    internal static class BoxResponseFactory
    {
        /// <summary>
        /// 指定したリポジトリの処理結果に適した、BOX レスポンスを作成します。
        /// </summary>
        /// <typeparam name="T">コンテンツ ボディに設定するオブジェクトの型。</typeparam>
        /// <param name="result">リポジトリの処理結果。</param>
        /// <returns>作成された BOX レスポンス。</returns>
        public static T CreateError<T>(RepositoryResult result)
            where T : BoxResponse, new()
        {
            return new T
            {
                ErrorMessage = result.ErrorMessage
            };
        }
    }
}