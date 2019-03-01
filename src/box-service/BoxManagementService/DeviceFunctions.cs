using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BoxManagementService
{
    /// <summary>
    /// デバイス管理Functions
    /// Durable Functionsを利用してBOXの状態管理を行います。
    /// </summary>
    public static class DeviceFunctions
    {
        /// <summary>
        /// デバイス管理アクティビティ
        /// </summary>
        public enum DeviceActivity
        {
            /// <summary>
            /// なし
            /// </summary>
            None = 0,

            /// <summary>
            /// 開放
            /// </summary>
            Open = 1,

            /// <summary>
            /// 商品更新
            /// </summary>
            UpdateItem = 2,

            /// <summary>
            /// 閉鎖
            /// </summary>
            Close = 3,

            /// <summary>
            /// 入力最終行
            /// </summary>
            Eol = 99
        }

        /// <summary>
        /// デバイスのメッセージを分析してデバイスの状態を更新するオーケストレータ
        /// 状態更新によりアクションが発生する場合に、対応するアクティビティを呼び出します。
        /// </summary>
        /// <param name="context">オーケストレーションコンテキスト</param>
        /// <param name="log">ログ</param>
        /// <returns>非同期タスク</returns>
        [FunctionName("UpdateDeviceStatus")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger log)
        {
            var data = context.GetInput<InputData>() ?? new InputData();
            if (data.State == null)
            {
                data.State = new DeviceState();
            }

            if (data.Input == null || !data.Input.Any())
            {
                return;
            }

            if (data.State.IsInitial || data.State.IsTerminal)
            {
                data.State.Update(null);
            }

            var inputs = data.Input.OrderBy(i => i.Timestamp).ToList();
            inputs.Add(null);
            var beginIndex = -1;
            var prevActivity = DeviceActivity.None;
            for (var i = 0; i < inputs.Count; i++)
            {
                var input = inputs[i];
                var activity = input == null ? DeviceActivity.Eol : data.State.Update(input);
                if (activity == prevActivity)
                {
                    continue;
                }

                switch (prevActivity)
                {
                    case DeviceActivity.None:
                        break;
                    case DeviceActivity.Open:
                        await context.CallActivityAsync("UpdateOpeningStocks", inputs.GetRange(beginIndex, i - beginIndex));
                        break;
                    case DeviceActivity.UpdateItem:
                        await context.CallActivityAsync("UpdateItems", inputs.GetRange(beginIndex, i - beginIndex));
                        break;
                    case DeviceActivity.Close:
                        await context.CallActivityAsync("UpdateClosingStocks", inputs.GetRange(beginIndex, i - beginIndex));
                        break;
                }

                beginIndex = i;
                prevActivity = activity;
            }

            context.SetCustomStatus(data.State);
        }

        /*
        /// <summary>
        /// デバイス管理用オーケストレーションクライアント
        /// ASAからのメッセージを、シングルトンオーケストレータを利用して処理します。
        /// </summary>
        /// <param name="req">Httpリクエスト</param>
        /// <param name="client">オーケストレーションクライアント</param>
        /// <param name="log">ロガー</param>
        /// <returns>レスポンス</returns>
        [FunctionName("DeviceFunctions_ReceiveDeviceMessage")]
        public static async Task<HttpResponseMessage> ReceiveDevicdMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient client,
            ILogger log)
        {
            var eventData = await req.Content.ReadAsAsync<IEnumerable<dynamic>>();
            var tasks = eventData
                .GroupBy(data => (string)data.box_id)
                .Select(data => StartUpdateDeviceStatusAsync(client, data.Key, data.ToList()));
            await Task.WhenAll(tasks);

            //return client.CreateCheckStatusResponse(req, instanceId);
            return req.CreateResponse(HttpStatusCode.OK);
        }
        */

        /// <summary>
        /// デバイス管理用オーケストレーションクライアント
        /// IoT Hubからのメッセージを、シングルトンオーケストレータを利用して処理します。
        /// </summary>
        /// <param name="messages">IoT Hubメッセージ</param>
        /// <param name="client">オーケストレーションクライアント</param>
        /// <param name="log">ロガー</param>
        /// <returns>非同期タスク</returns>
        [FunctionName("DeviceFunctions_ReceiveDeviceMessageFromIotHub")]
        [Singleton]
        public static async Task ReceiveDeviceMessageFromIotHub(
            [EventHubTrigger("retail-test-iot-hub-dev", Connection = "IotHubEventConnectionString")]EventData[] messages,
            [OrchestrationClient]DurableOrchestrationClient client,
            ILogger log)
        {
            var jsonMessages = messages
                .Select(m => (dynamic)JObject.Parse(Encoding.UTF8.GetString(m.Body)))
                .ToArray();

            foreach (var msg in jsonMessages)
            {
                log.LogInformation($"EventHubTrigger received message : {msg}");
            }

            await ReceiveDeviceMessages(client, jsonMessages, log);
        }

        /// <summary>
        /// デバイスメッセージ受付処理
        /// </summary>
        /// <param name="client">オーケストレーションクライアント</param>
        /// <param name="messages">デバイスメッセージ(JSON)</param>
        /// <param name="log">ロガー</param>
        /// <returns>レスポンス</returns>
        private static async Task ReceiveDeviceMessages(
            DurableOrchestrationClient client,
            IEnumerable<dynamic> messages,
            ILogger log)
        {
            var eventData = messages
                .Select(m => ConvertTypedMessage(m))
                .Where(m => m.Type != null)
                .ToArray();

            var tasks = eventData
                .GroupBy(data => (string)data.box_id)
                .Select(data => StartUpdateDeviceStatusAsync(client, data.Key, data.ToList()));
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// デバイスメッセージにメッセージタイプを付けます
        /// </summary>
        /// <param name="message">デバイスメッセージ</param>
        /// <returns>タイプ付きデバイスメッセージ</returns>
        private static dynamic ConvertTypedMessage(dynamic message)
        {
            message.Type = (string)null;

            if (message.items != null)
            {
                if (message.message_type == "stock_start")
                {
                    message.Type = "open";
                }
                else if (message.message_type == "stock_diff_trading")
                {
                    message.Type = "move";
                }
                else if (message.message_type == "stock_end")
                {
                    message.Type = "close";
                }
            }

            return message;
        }

        /// <summary>
        /// 単一のデバイス情報更新オーケストレータを起動します。
        /// </summary>
        /// <param name="client">オーケストレーションクライアント</param>
        /// <param name="instanceId">オーケストレータインスタンスID</param>
        /// <param name="data">入力データ</param>
        /// <returns>非同期タスク</returns>
        private static async Task StartUpdateDeviceStatusAsync(DurableOrchestrationClient client, string instanceId, IEnumerable<dynamic> data)
        {
            var existingInstance = await client.GetStatusAsync(instanceId);
            if (existingInstance != null)
            {
                while (
                    existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Pending
                    || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Running)
                {
                    await Task.Delay(100);
                    existingInstance = await client.GetStatusAsync(instanceId);
                }
            }

            await client.StartNewAsync(
                "UpdateDeviceStatus",
                instanceId,
                new InputData
                {
                    State = existingInstance?.CustomStatus?.ToObject<DeviceState>(),
                    Input = data
                });

            /*
            var output = await starter.WaitForCompletionOrCreateCheckStatusResponseAsync(
                req,
                instanceId,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(1));
            */
        }

        /// <summary>
        /// デバイス管理用オーケストレータリセット
        /// </summary>
        /// <param name="req">Httpリクエスト</param>
        /// <param name="deviceId">リセット対象のデバイスID</param>
        /// <param name="starter">オーケストレーションクライアント</param>
        /// <param name="log">ロガー</param>
        /// <returns>レスポンス</returns>
        [FunctionName("ResetDeviceStatus")]
        public static async Task<HttpResponseMessage> ResetDeviceStatus(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "v1/devices/{deviceId}/status/reset")]HttpRequestMessage req,
            string deviceId,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            var instanceId = deviceId;
            var existingInstance = await starter.GetStatusAsync(instanceId);
            if (existingInstance != null)
            {
                await starter.TerminateAsync(instanceId, "reset command called.");
                //await starter.PurgeInstanceHistoryAsync(instanceId);  // これを呼ぶとInstanceの情報すべてが削除される。この場合StartNewAsyncとWaitForCompletionOrCreateCheckStatusResponseAsyncの必要はない
                await starter.StartNewAsync(
                    "UpdateDeviceStatus",
                    instanceId,
                    new InputData
                    {
                        State = existingInstance?.CustomStatus?.ToObject<DeviceState>(),
                        Input = new[]
                        {
                            new
                            {
                                Type = "terminate"
                            }
                        }
                    });

                var output = await starter.WaitForCompletionOrCreateCheckStatusResponseAsync(
                    req,
                    instanceId,
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(1));
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// オーケストレータの前回カスタムステータスと、ASAからの入力をまとめて入力情報とするクラス。
        /// </summary>
        public class InputData
        {
            /// <summary>
            /// 前回デバイスステータス
            /// </summary>
            public DeviceState State { get; set; }

            /// <summary>
            /// ASA入力
            /// </summary>
            public IEnumerable<dynamic> Input { get; set; }
        }
        
        /// <summary>
        /// デバイスの状態を保持するためのクラス
        /// オーケストレータのカスタム状態に保持するため、JSONにシリアル化することを想定している。
        /// </summary>
        public class DeviceState
        {
            /// <summary>
            /// デバイスの状態を表す列挙体
            /// </summary>
            public enum StateType
            {
                /// <summary>
                /// 初期状態
                /// </summary>
                Initial = 0,

                /// <summary>
                /// 閉鎖状態
                /// </summary>
                Close = 1,

                /// <summary>
                /// 開放状態
                /// </summary>
                Open = 3,

                /// <summary>
                /// 終了状態
                /// </summary>
                Terminal = 99,
            }

            /// <summary>
            /// デバイス状態
            /// </summary>
            public StateType State { get; set; } = StateType.Initial;

            /// <summary>
            /// 初期状態かどうかを返します。
            /// </summary>
            internal bool IsInitial => this.State == StateType.Initial;

            /// <summary>
            /// 終了状態かどうかを返します。
            /// </summary>
            internal bool IsTerminal => this.State == StateType.Terminal;

            /// <summary>
            /// 状態を更新し、アクティビティを返します。
            /// </summary>
            /// <param name="data">入力データ</param>
            /// <returns>アクティビティ</returns>
            public DeviceActivity Update(dynamic data)
            {
                if (this.IsTerminate(data))
                {
                    this.State = StateType.Terminal;
                    return DeviceActivity.None;
                }

                switch (this.State)
                {
                    case StateType.Initial:
                        this.State = StateType.Close;
                        return DeviceActivity.None;

                    case StateType.Close:
                        if (this.IsOpen(data))
                        {
                            this.State = StateType.Open;
                            return DeviceActivity.Open;
                        }
                        return DeviceActivity.None;

                    case StateType.Open:
                        if (this.IsItemMove(data))
                        {
                            return DeviceActivity.UpdateItem;
                        }

                        if (this.IsClose(data))
                        {
                            this.State = StateType.Close;
                            return DeviceActivity.Close;
                        }
                        return DeviceActivity.None;

                    case StateType.Terminal:
                        this.State = StateType.Close;
                        return DeviceActivity.None;

                    default:
                        return DeviceActivity.None;
                }
            }

            /// <summary>
            /// 開放データであることを確認します。
            /// </summary>
            /// <param name="data">入力データ</param>
            /// <returns>結果</returns>
            private bool IsOpen(dynamic data) => data?.Type == "open";

            /// <summary>
            /// 商品移動データであることを確認します。
            /// </summary>
            /// <param name="data">入力データ</param>
            /// <returns>結果</returns>
            private bool IsItemMove(dynamic data) => data?.Type == "move";

            /// <summary>
            /// 閉鎖データであることを確認します。
            /// </summary>
            /// <param name="data">入力データ</param>
            /// <returns>結果</returns>
            private bool IsClose(dynamic data) => data?.Type == "close";

            /// <summary>
            /// 終了データであることを確認します。
            /// </summary>
            /// <param name="data">入力データ</param>
            /// <returns>結果</returns>
            private bool IsTerminate(dynamic data) => data?.Type == "terminate";
        }
    }
}