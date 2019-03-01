using System;
using BoxManagementService.DataCotracts.Responses;
using BoxManagementService.Models.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BoxManagementService
{
    /// <summary>
    /// アクションの結果を作成します。
    /// </summary>
    internal static class ActionResultFactory
    {
        /// <summary>
        /// 指定したリポジトリの処理結果に適した、アクションのエラー結果を作成します。
        /// </summary>
        /// <typeparam name="T">コンテンツ ボディに設定するオブジェクトの型。</typeparam>
        /// <param name="result">リポジトリの処理結果。</param>
        /// <returns>作成されたアクションのエラー結果。</returns>
        public static IActionResult CreateError<T>(RepositoryResult result)
            where T : BoxResponse, new()
        {
            var value = BoxResponseFactory.CreateError<T>(result);

            return CreateError(result, value);
        }

        /// <summary>
        /// 指定したリポジトリの処理結果に適した、アクションのエラー結果を作成します。
        /// </summary>
        /// <param name="result">リポジトリの処理結果。</param>
        /// <param name="value">コンテンツ ボディに設定するオブジェクト。</param>
        /// <returns>作成されたアクションのエラー結果。</returns>
        public static IActionResult CreateError(RepositoryResult result, object value)
        {
            switch (result.Status)
            {
                case RepositoryResult.ResultStatus.NotFound:
                    return new NotFoundObjectResult(value);

                case RepositoryResult.ResultStatus.Conflict:
                    return new ConflictObjectResult(value);

                case RepositoryResult.ResultStatus.InvalidParameter:
                    return new BadRequestObjectResult(value);

                default:
                    throw new ArgumentException($"Property '{nameof(RepositoryResult.Status)}' has invalid value.", nameof(result));
            }
        }
    }
}