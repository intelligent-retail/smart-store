using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PosService.Constants;
using PosService.DataContracts.Items.Responses;
using PosService.DataContracts.Pos.Parameters;
using PosService.DataContracts.Pos.Responses;
using PosService.Models;
using PosService.Models.Documents;
using PosService.Models.Repositories.CosmosDB;
using PosService.Models.Repositories.Http;
using PosService.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using static PosService.DataContracts.Items.Parameters.UpdateStocksParameter;
using static PosService.Models.Documents.TransactionLogBase;

namespace PosService
{
    /// <summary>
    /// POS ロジック
    /// </summary>
    public class PosLogic : IPosLogic
    {
        /// <summary>標準的な OK を表す結果</summary>
        private ObjectResult Ok => new OkObjectResult("OK");

        /// <summary>ロガー</summary>
        private ILogger _log;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="log">ロガー</param>
        public PosLogic(ILogger log) => this._log = log;

        /// <summary>
        /// カートを作成します
        /// </summary>
        /// <param name="param">カート作成パラメーター</param>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <returns>
        /// 作成に成功した時
        /// 　201:Created
        /// 　　　Content に CreateCartResponse が設定されます
        /// 作成に失敗した時
        /// 　パラメーター誤り
        /// 　　400:BadRequest
        /// </returns>
        public async Task<ObjectResult> CreateCartAsync(CreateCartParameter param, DocumentClient client)
        {
            var company = await new CompanyMasterRepository(client).GetCompanyAsync(param.CompanyCode);
            if (company == null)
            {
                return new BadRequestObjectResult(Errors.ParameterError);
            }

            var store = await new StoreMasterRepository(client).GetStoreAsync(param.CompanyCode, param.StoreCode);
            if (store == null)
            {
                return new BadRequestObjectResult(Errors.ParameterError);
            }

            var terminal = await new TerminalMasterRepository(client).GetTerminalAsync(param.CompanyCode, param.StoreCode, param.TerminalNo);
            if (terminal == null)
            {
                return new BadRequestObjectResult(Errors.ParameterError);
            }

            var receiptNo = await new CounterRepository(client).NumberingCount(param.CompanyCode, param.StoreCode, param.TerminalNo, CounterType.Receipt.Code);
            var transactionNo = await new CounterRepository(client).NumberingCount(param.CompanyCode, param.StoreCode, param.TerminalNo, CounterType.Transaction.Code);

            var cartRepo = new CartRepository(client);
            var cart = await cartRepo.CreateCartAsync(
                param.CompanyCode,
                param.StoreCode,
                store.StoreName,
                param.TerminalNo,
                param.UserId,
                param.UserName,
                receiptNo,
                transactionNo);

            return new CreatedResult(string.Empty, new CreateCartResponse { CartId = cart.CartId });
        }

        /// <summary>
        /// カートを取得します
        /// </summary>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <returns>
        /// 取得に成功した時
        /// 　200:OK
        /// 　　　Content に GetCartResponse が設定されます
        /// 取得に失敗した時
        /// 　指定したカート ID に該当するカートが存在しない
        /// 　　404:NotFound
        /// </returns>
        public async Task<ObjectResult> GetCartAsync(DocumentClient client, string cartId)
        {
            var cartRepo = new CartRepository(client);
            var cart = await cartRepo.GetCartAsync(cartId);
            if (cart == null)
            {
                return new NotFoundObjectResult(Errors.CartNotFound);
            }

            var depositTotal = cart.Payments.Sum(p => p.DepositAmount);
            var balance = Math.Max(cart.Sales.TotalAmountWithTaxes - depositTotal, 0);
            var response = new GetCartResponse
            {
                Store = new GetCartResponse.StoreInfo
                {
                    StoreCode = cart.StoreCode,
                    StoreName = cart.StoreName,
                    TerminalNo = cart.TerminalNo
                },
                User = new GetCartResponse.UserInfo
                {
                    UserId = cart.User.UserId,
                    UserName = cart.User.UserName
                },
                Cart = new GetCartResponse.CartInfo
                {
                    CartId = cart.CartId,
                    TotalAmount = cart.Sales.TotalAmountWithTaxes,
                    SubtotalAmount = cart.Sales.TotalAmount,
                    TotalQuantity = cart.Sales.TotalQuantity,
                    DepositAmount = depositTotal,
                    ChangeAmount = cart.Sales.ChangeAmount,
                    Balance = balance,
                    ReceiptNo = cart.ReceiptNo,
                    ReceiptText = cart.ReceiptText,
                    TransactionNo = cart.TransactionNo,
                    CartStatus = cart.Status,
                    LineItems = new List<GetCartResponse.CartInfo.LineItem>(),
                    Payments = new List<GetCartResponse.CartInfo.Payment>(),
                    Taxes = new List<GetCartResponse.CartInfo.Tax>()
                }
            };

            foreach (var lineItem in cart.LineItems)
            {
                if (lineItem.Quantity <= 0)
                {
                    continue;
                }

                response.Cart.LineItems.Add(new GetCartResponse.CartInfo.LineItem
                {
                    LineNo = lineItem.LineNo,
                    ItemCode = lineItem.ItemCode,
                    ItemName = lineItem.Description,
                    UnitPrice = lineItem.UnitPrice,
                    Quantity = lineItem.Quantity,
                    Amount = lineItem.Amount,
                    ImageUrls = lineItem.ImageUrls
                });
            }

            foreach (var payment in cart.Payments)
            {
                response.Cart.Payments.Add(new GetCartResponse.CartInfo.Payment
                {
                    PaymentNo = payment.PaymentNo,
                    PaymentCode = payment.PaymentCode,
                    PaymentName = payment.Description,
                    PaymentAmount = payment.Amount
                });
            }

            foreach (var tax in cart.Taxes)
            {
                response.Cart.Taxes.Add(new GetCartResponse.CartInfo.Tax
                {
                    TaxNo = tax.TaxNo,
                    TaxName = cart.Masters.Taxes.Where(t => t.TaxCode == tax.TaxCode).First().Description,
                    TaxAmount = tax.TaxAmount
                });
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// カートに商品を追加します
        /// </summary>
        /// <param name="param">商品追加パラメーター</param>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <returns>
        /// 追加に成功した時
        /// 　200:OK
        /// 追加に失敗した時
        /// 　パラメーター誤り または
        /// 　カート状態が処理を受け付けることができない または
        /// 　在庫情報の更新ができない
        /// 　　400:BadRequest
        /// 　指定したカート ID に該当するカートが存在しない または
        /// 　指定した商品が存在しない
        /// 　　404:NotFound
        /// </returns>
        public async Task<ObjectResult> AddItemsToCartAsync(AddItemsParameter param, DocumentClient client, string cartId)
        {
            var cartRepo = new CartRepository(client);
            var cart = await cartRepo.GetCartAsync(cartId);
            if (cart == null)
            {
                return new NotFoundObjectResult(Errors.CartNotFound);
            }

            if (cart.Status != CartStatus.EnteringItem.Code)
            {
                return new BadRequestObjectResult(Errors.BadCartStatus);
            }

            var addResult = await this.AddItemsToCartAsync(cart, param.Items, true);
            if (!(addResult is OkObjectResult))
            {
                return addResult;
            }

            await cartRepo.UpdateAsync(cart);
            return this.Ok;
        }

        /// <summary>
        /// カートから商品を削除します
        /// </summary>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <param name="itemCode">商品コード</param>
        /// <param name="quantity">削除する数量</param>
        /// <returns>
        /// 削除に成功した時
        /// 　200:OK
        /// 削除に失敗した時
        /// 　パラメーター誤り または
        /// 　カート状態が処理を受け付けることができない または
        /// 　カート内の商品数量を超える返品数量を指定した または
        /// 　在庫情報の更新ができない
        /// 　　400:BadRequest
        /// 　指定したカート ID に該当するカートが存在しない
        /// 　　404:NotFound
        /// </returns>
        public async Task<ObjectResult> DeleteItemFromCartAsync(
            DocumentClient client,
            string cartId,
            string itemCode,
            int quantity)
        {
            var cartRepo = new CartRepository(client);
            var cart = await cartRepo.GetCartAsync(cartId);
            if (cart == null)
            {
                return new NotFoundObjectResult(Errors.CartNotFound);
            }

            if (cart.Status != CartStatus.EnteringItem.Code)
            {
                return new BadRequestObjectResult(Errors.BadCartStatus);
            }

            // 最後に追加した明細から該当商品を検索し、数量を減算していく
            var rest = quantity;

            foreach (var lineItem in cart.LineItems.Where(i => i.ItemCode == itemCode).OrderByDescending(l => l.LineNo))
            {
                if (lineItem.Quantity >= rest)
                {
                    lineItem.Quantity -= rest;
                    lineItem.ReturnedQuantity += rest;
                    rest = 0;
                    break;
                }
                else
                {
                    rest -= lineItem.Quantity;
                    lineItem.ReturnedQuantity += lineItem.Quantity;
                    lineItem.Quantity = 0;
                }
            }

            if (rest > 0)
            {
                return new BadRequestObjectResult(Errors.CannotDeleteItem);
            }

            this.Subtotal(cart);

            // 在庫仮引当の取消
            var stockRepo = new StocksRepository(StaticHttpClient.Instance);
            var stockResult = await stockRepo.UpdateStocksAsync(
                cart,
                new List<StockItem> { new StockItem { LineNo = 1, ItemCode = itemCode, Quantity = -1 * quantity } },
                StockAllocationType.ProvisionalAllocation,
                this._log);
            if (!stockResult)
            {
                return new BadRequestObjectResult(Errors.CannotUpdateStocks);
            }

            await cartRepo.UpdateAsync(cart);
            return this.Ok;
        }

        /// <summary>
        /// カートを小計します
        /// </summary>
        /// <param name="param">小計パラメーター</param>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <returns>
        /// 小計に成功した時
        /// 　200:OK
        /// 小計に失敗した時
        /// 　パラメーター誤り または
        /// 　カート状態が処理を受け付けることができない または
        /// 　在庫情報の更新ができない
        /// 　　400:BadRequest
        /// 　指定したカート ID に該当するカートが存在しない または
        /// 　指定した商品が存在しない
        /// 　　404:NotFound
        /// </returns>
        public async Task<ObjectResult> SubtotalCartAsync(SubtotalParameter param, DocumentClient client, string cartId)
        {
            var cartRepo = new CartRepository(client);
            var cart = await cartRepo.GetCartAsync(cartId);
            if (cart == null)
            {
                return new NotFoundObjectResult(Errors.CartNotFound);
            }

            if (cart.Status != CartStatus.EnteringItem.Code)
            {
                return new BadRequestObjectResult(Errors.BadCartStatus);
            }

            cart.Status = CartStatus.Paying.Code;

            if (param.Items?.Count > 0)
            {
                // 小計時の商品明細指定は全明細洗い替え
                // ※取引前後の棚状況の差分を正とするシステム向け機能
                var currentLineItems = cart.LineItems;

                // 明細の洗替 (PLU 時のマスタはキャッシュしている。キャッシュしたマスタの価格を採用)
                cart.LineItems = new List<CartDocument.CartLineItem>();
                var result = await this.AddItemsToCartAsync(cart, param.Items, false);
                if (!(result is OkObjectResult))
                {
                    return result;
                }

                // 在庫仮引当の調整
                if (false == await this.AdjustStocks(cart, currentLineItems, cart.LineItems))
                {
                    return new BadRequestObjectResult(Errors.CannotUpdateStocks);
                }
            }

            await cartRepo.UpdateAsync(cart);
            return this.Ok;
        }

        /// <summary>
        /// カートに支払を追加します
        /// </summary>
        /// <param name="param">支払追加パラメーター</param>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <returns>
        /// 追加に成功した時
        /// 　200:OK
        /// 追加に失敗した時
        /// 　パラメーター誤り または
        /// 　カート状態が処理を受け付けることができない
        /// 　　400:BadRequest
        /// 　指定したカート ID に該当するカートが存在しない
        /// 　　404:NotFound
        /// </returns>
        public async Task<ObjectResult> AddPaymentsToCartAsync(AddPaymentsParameter param, DocumentClient client, string cartId)
        {
            var cartRepo = new CartRepository(client);
            var cart = await cartRepo.GetCartAsync(cartId);
            if (cart == null)
            {
                return new NotFoundObjectResult(Errors.CartNotFound);
            }

            if (cart.Status != CartStatus.Paying.Code)
            {
                return new BadRequestObjectResult(Errors.BadCartStatus);
            }

            // マスターから支払種別の名称を取得する
            var namesDictionary = new Dictionary<string, string>();
            foreach (var pay in param.Payments)
            {
                if (!namesDictionary.ContainsKey(pay.PaymentCode))
                {
                    var master = await new PaymentMasterRepository(client).GetPaymentAsync(
                        cart.CompanyCode,
                        cart.StoreCode,
                        pay.PaymentCode);
                    if (master == null)
                    {
                        return new BadRequestObjectResult(Errors.ParameterError);
                    }

                    namesDictionary.Add(pay.PaymentCode, master.Description);
                }
            }

            // 現在の小計を計算
            this.Subtotal(cart);

            // 預り金額は既存支払の合計
            var depositTotal = cart.Payments.Sum(p => p.DepositAmount);
            var balance = cart.Sales.TotalAmountWithTaxes - depositTotal;
            int paymentNo = cart.Payments.Count + 1;

            // 新規支払の追加
            foreach (var payment in param.Payments)
            {
                if (balance <= 0)
                {
                    // 0 円以下の状態からの追加は許可しない
                    return new BadRequestObjectResult(Errors.ParameterError);
                }

                // 金額指定なしは合計金額ピッタリを指定した扱いとする
                decimal depositAmount = payment.Amount == 0
                    ? cart.Sales.TotalAmountWithTaxes
                    : payment.Amount;

                depositTotal += depositAmount;

                cart.Payments.Add(new Payment
                {
                    PaymentNo = paymentNo,
                    PaymentCode = payment.PaymentCode,
                    DepositAmount = depositAmount,
                    Amount = balance >= depositAmount ? depositAmount : balance,
                    Description = namesDictionary[payment.PaymentCode]
                });

                balance -= depositAmount;
                paymentNo++;
            }

            // HACK : 支払種別を考慮する (現状では考慮していないため、クレジットでもつり銭が出る)
            //        UI側でつり銭が出ない制御を行う or POSサービス側で
            cart.Sales.ChangeAmount = depositTotal - cart.Sales.TotalAmountWithTaxes;
            if (cart.Sales.ChangeAmount < 0)
            {
                cart.Sales.ChangeAmount = 0;
            }

            await cartRepo.UpdateAsync(cart);
            return this.Ok;
        }

        /// <summary>
        /// カートの取引を確定します
        /// </summary>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <returns>
        /// 取引確定に成功した時
        /// 　200:OK
        /// 取引確定に失敗した時
        /// 　カート状態が処理を受け付けることができない または
        /// 　在庫情報の更新ができない
        /// 　　400:BadRequest
        /// 　指定したカート ID に該当するカートが存在しない
        /// 　　404:NotFound
        /// </returns>
        public async Task<ObjectResult> BillCartAsync(DocumentClient client, string cartId)
        {
            var cartRepo = new CartRepository(client);
            var cart = await cartRepo.GetCartAsync(cartId);
            if (cart == null)
            {
                return new NotFoundObjectResult(Errors.CartNotFound);
            }

            if (cart.Status != CartStatus.Paying.Code)
            {
                return new BadRequestObjectResult(Errors.BadCartStatus);
            }

            cart.Status = CartStatus.Completed.Code;
            cart.TransactionType = TransactionType.NormalSales.TypeNo;
            cart.Sales.IsCanceled = false;

            var tranRepo = new TransactionLogRepository(client);
            var tranDoc = await tranRepo.CreateTransactionLogAsync(cart);

            var receiptRepo = new ReceiptRepository(client);
            var receiptDoc = await receiptRepo.CreateReceiptAsync(cart);

            cart.TransactionNo = tranDoc.TransactionNo;
            cart.BusinessDate = tranDoc.BusinessDate;
            cart.ReceiptNo = tranDoc.ReceiptNo;
            cart.ReceiptText = receiptDoc.ReceiptText;

            // 在庫の引当
            var stockItems = this.GetStockItemsFromLineItems(cart.LineItems, false);
            if (stockItems.Count > 0)
            {
                var stockRepo = new StocksRepository(StaticHttpClient.Instance);
                var stockResult = await stockRepo.UpdateStocksAsync(
                    cart,
                    stockItems,
                    StockAllocationType.Allocation,
                    this._log);
                if (!stockResult)
                {
                    return new BadRequestObjectResult(Errors.CannotUpdateStocks);
                }
            }
            else
            {
                // 引当対象なしの場合は取引中止すべきなので、Bill ではエラー扱いとする
                this._log.LogError("引当対象なし");
                return new BadRequestObjectResult(Errors.BadCartStatus);
            }

            await cartRepo.UpdateAsync(cart);
            return this.Ok;
        }

        /// <summary>
        /// カートの取引を中止します
        /// </summary>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <returns>
        /// 取引中止に成功した時
        /// 　200:OK
        /// 取引中止に失敗した時
        /// 　カート状態が処理を受け付けることができない または
        /// 　在庫情報の更新ができない
        /// 　　400:BadRequest
        /// 　指定したカート ID に該当するカートが存在しない
        /// 　　404:NotFound
        /// </returns>
        public async Task<ObjectResult> CancelCartAsync(DocumentClient client, string cartId)
        {
            var cartRepo = new CartRepository(client);
            var cart = await cartRepo.GetCartAsync(cartId);
            if (cart == null)
            {
                return new NotFoundObjectResult(Errors.CartNotFound);
            }

            // カート情報はしばらく CosmosDB に残るため、取引が終わった後の命令は不許可とする
            if (cart.Status == CartStatus.Completed.Code
                || cart.Status == CartStatus.Canceled.Code)
            {
                return new BadRequestObjectResult(Errors.BadCartStatus);
            }

            cart.Status = CartStatus.Canceled.Code;
            cart.TransactionType = TransactionType.CanceledSales.TypeNo;
            cart.Sales.IsCanceled = true;

            var tranRepo = new TransactionLogRepository(client);
            var tranDoc = await tranRepo.CreateTransactionLogAsync(cart);

            var receiptRepo = new ReceiptRepository(client);
            var receiptDoc = await receiptRepo.CreateReceiptAsync(cart);

            cart.TransactionNo = tranDoc.TransactionNo;
            cart.BusinessDate = tranDoc.BusinessDate;
            cart.ReceiptNo = tranDoc.ReceiptNo;
            cart.ReceiptText = receiptDoc.ReceiptText;

            // 在庫仮引当の取消
            var stockItems = this.GetStockItemsFromLineItems(cart.LineItems, true);
            if (stockItems.Count > 0)
            {
                var stockRepo = new StocksRepository(StaticHttpClient.Instance);
                var stockResult = await stockRepo.UpdateStocksAsync(
                    cart,
                    stockItems,
                    StockAllocationType.ProvisionalAllocation,
                    this._log);
                if (!stockResult)
                {
                    return new BadRequestObjectResult(Errors.CannotUpdateStocks);
                }
            }

            await cartRepo.UpdateAsync(cart);
            return this.Ok;
        }

        /// <summary>
        /// カートに商品を追加します
        /// </summary>
        /// <param name="cart">カート Document</param>
        /// <param name="items">追加する商品</param>
        /// <param name="updatesStocks">在庫更新処理を行うかどうか</param>
        /// <returns>
        /// 追加に成功した時
        /// 　200:OK
        /// 追加に失敗した時
        /// 　在庫情報の更新ができない
        /// 　　400:BadRequest
        /// 　指定した商品が存在しない
        /// 　　404:NotFound
        /// </returns>
        private async Task<ObjectResult> AddItemsToCartAsync(CartDocument cart, List<ItemParametersDetail> items, bool updatesStocks)
        {
            // 今回の取引で参照していない商品情報を取得する
            var pluTargets = new List<string>();
            foreach (var item in items)
            {
                var itemMaster = cart.Masters.Items.Where(i => i.ItemCode == item.ItemCode).FirstOrDefault();
                if (itemMaster == null && !pluTargets.Contains(item.ItemCode))
                {
                    pluTargets.Add(item.ItemCode);
                }
            }

            if (pluTargets.Count > 0)
            {
                var itemsRepo = new ItemsRepository(StaticHttpClient.Instance);
                var itemResponse = await itemsRepo.GetItemsAsync(cart.CompanyCode, cart.StoreCode, pluTargets, this._log);
                if (itemResponse == null || itemResponse.Items == null || itemResponse.Items.Count < pluTargets.Count)
                {
                    return new NotFoundObjectResult(Errors.ItemNotFound);
                }

                this.UpdateReferenceMasters(cart, itemResponse);
            }

            // カートの商品明細に追加する
            var cartLineNo = cart.LineItems.Count + 1;
            foreach (var item in items)
            {
                var itemMaster = cart.Masters.Items.Where(i => i.ItemCode == item.ItemCode).FirstOrDefault();

                cart.LineItems.Add(new CartDocument.CartLineItem
                {
                    LineNo = cartLineNo,
                    ItemCode = itemMaster.ItemCode,
                    DepartmentCode = itemMaster.DepartmentCode,
                    Description = itemMaster.Description,
                    DescriptionShort = itemMaster.DescriptionShort,
                    UnitPrice = itemMaster.UnitPrice,
                    Quantity = item.Quantity,
                    ReturnedQuantity = 0,
                    TaxCode = itemMaster.TaxCode,
                    ItemDetails = itemMaster.ItemDetails,
                    ImageUrls = itemMaster.ImageUrls
                });

                cartLineNo++;
            }

            // 小計する
            this.Subtotal(cart);

            if (updatesStocks)
            {
                // 在庫を仮引当する (※可能な限りロールバックが不要となるよう、最後に実施)
                var stockItems = this.GetStockItemsFromLineItems(items, false);
                if (stockItems.Count <= 0)
                {
                    return new BadRequestObjectResult(Errors.CannotUpdateStocks);
                }

                var stockRepo = new StocksRepository(StaticHttpClient.Instance);
                var stockResult = await stockRepo.UpdateStocksAsync(
                    cart,
                    stockItems,
                    StockAllocationType.ProvisionalAllocation,
                    this._log);
                if (!stockResult)
                {
                    return new BadRequestObjectResult(Errors.CannotUpdateStocks);
                }
            }

            // HACK 要件に応じて、呼出元でのカート情報更新に失敗した場合の仮引当のロールバックが必要
            return this.Ok;
        }

        /// <summary>
        /// 参照マスターを更新します
        /// </summary>
        /// <param name="cart">カート Document</param>
        /// <param name="res">商品情報取得結果</param>
        private void UpdateReferenceMasters(CartDocument cart, GetItemsResponse res)
        {
            foreach (var item in res.Items)
            {
                if (!cart.Masters.Items.Any(i => i.ItemCode == item.ItemCode))
                {
                    // HACK : 要件に応じて、商品マスタ 対 階層マスタ、商品マスタ 対 税マスタのリレーションに矛盾がないかチェック
                    var minMd = item.MdHierarchies.OrderByDescending(m => m.MdHierarchyLevel).First();

                    cart.Masters.Items.Add(new ItemMaster
                    {
                        CompanyCode = item.CompanyCode,
                        StoreCode = item.StoreCode,
                        ItemCode = item.ItemCode,
                        Description = item.Description,
                        DescriptionShort = item.DescriptionShort,
                        DescriptionLong = item.DescriptionLong,
                        ManufacturerCode = item.ManufacturerCode,
                        UnitPrice = item.UnitPrice,
                        UnitCost = item.UnitCost,
                        EntryDate = item.EntryDate,
                        LastUpdateDate = item.LastUpdateDate,
                        ItemDetails = item.ItemDetails,
                        ImageUrls = item.ImageUrls,
                        ParentCode = minMd.MdHierarchyCode,
                        ParentLevel = minMd.MdHierarchyLevel,
                        DepartmentCode = minMd.ParentCode,
                        TaxCode = item.Tax.TaxCode
                    });
                }

                foreach (var md in item.MdHierarchies)
                {
                    if (!cart.Masters.MdHierarchies.Any(m => m.MdHierarchyCode == md.MdHierarchyCode && m.MdHierarchyLevel == md.MdHierarchyLevel))
                    {
                        cart.Masters.MdHierarchies.Add(md);
                    }
                }

                if (!cart.Masters.Taxes.Any(t => t.TaxCode == item.Tax.TaxCode))
                {
                    cart.Masters.Taxes.Add(item.Tax);
                }
            }
        }

        /// <summary>
        /// 小計します
        /// </summary>
        /// <param name="cart">カート Document</param>
        private void Subtotal(CartDocument cart)
        {
            cart.Sales.TotalAmountWithTaxes = 0;
            cart.Sales.TotalQuantity = 0;

            var dic = new Dictionary<string, Tax>();

            int taxNo = 1;
            foreach (var cartItem in cart.LineItems)
            {
                cartItem.Amount = cartItem.UnitPrice * cartItem.Quantity;

                cart.Sales.TotalAmountWithTaxes += cartItem.Amount;
                cart.Sales.TotalQuantity += cartItem.Quantity;

                Tax tax;
                if (!dic.TryGetValue(cartItem.TaxCode, out tax))
                {
                    var tm = cart.Masters.Taxes.First(t => t.TaxCode == cartItem.TaxCode);

                    // HACK : 内税 8％ 以外をサポートする場合は辞書へ追加する
                    if (tm.TaxType != TaxType.IncludedTax.Code
                        || tm.Rate != 8m)
                    {
                        throw new NotImplementedException();
                    }

                    tax = new Tax { TaxCode = cartItem.TaxCode, TaxNo = taxNo };
                    dic[cartItem.TaxCode] = tax;

                    taxNo++;
                }

                tax.TargetAmount += cartItem.Amount;
                tax.TargetQuantity += cartItem.Quantity;
            }

            // HACK : 内税固定　8％ 限定で税計算している
            decimal rate = 8;
            decimal includedTaxesAmount = 0;
            decimal excludedTaxesAmount = 0;
            foreach (var tax in dic.Values)
            {
                tax.TaxAmount = tax.TargetAmount - (int)Math.Floor(tax.TargetAmount * 100 / (100 + rate));
                includedTaxesAmount += tax.TaxAmount;
            }

            cart.Taxes = dic.Values.OrderBy(t => t.TaxNo).ToList();

            decimal taxesAmount = includedTaxesAmount + excludedTaxesAmount;

            // HACK : 内税限定で値引きもない前提では TotalAmount == TotalAmountWithTaxes が成立する
            cart.Sales.TotalAmount = cart.Sales.TotalAmountWithTaxes;
            cart.Sales.TaxesAmount = taxesAmount;
        }

        /// <summary>
        /// 明細情報から在庫管理へ連携すべき情報を取得します
        /// </summary>
        /// <param name="items">明細情報</param>
        /// <param name="isCancel">キャンセルの場合は true、そうでない場合は false</param>
        /// <returns>在庫管理へ連携すべき情報</returns>
        private List<StockItem> GetStockItemsFromLineItems(List<CartDocument.CartLineItem> items, bool isCancel) =>
            this.GetStockItemsFromLineItems(
                items.Select(i => (Code: i.ItemCode, Qty: i.Quantity)).ToList(),
                isCancel);

        /// <summary>
        /// 明細情報から在庫管理へ連携すべき情報を取得します
        /// </summary>
        /// <param name="items">明細情報</param>
        /// <param name="isCancel">キャンセルの場合は true、そうでない場合は false</param>
        /// <returns>在庫管理へ連携すべき情報</returns>
        private List<StockItem> GetStockItemsFromLineItems(List<ItemParametersDetail> items, bool isCancel) =>
            this.GetStockItemsFromLineItems(
                items.Select(i => (Code: i.ItemCode, Qty: i.Quantity)).ToList(),
                isCancel);

        /// <summary>
        /// 明細情報から在庫管理へ連携すべき情報を取得します
        /// </summary>
        /// <param name="items">明細情報</param>
        /// <param name="isCancel">キャンセルの場合は true、そうでない場合は false</param>
        /// <returns>在庫管理へ連携すべき情報</returns>
        private List<StockItem> GetStockItemsFromLineItems(List<(string Code, int Qty)> items, bool isCancel)
        {
            var dic = new Dictionary<string, int>();

            if (isCancel)
            {
                foreach (var item in items)
                {
                    dic.TryGetValue(item.Code, out int qty);
                    dic[item.Code] = qty - item.Qty;
                }
            }
            else
            {
                foreach (var item in items)
                {
                    dic.TryGetValue(item.Code, out int qty);
                    dic[item.Code] = qty + item.Qty;
                }
            }

            var stockItems = new List<StockItem>();
            var lineNo = 1;

            foreach (var kvp in dic)
            {
                if (kvp.Value == 0)
                {
                    continue;
                }

                stockItems.Add(new StockItem
                {
                    LineNo = lineNo,
                    ItemCode = kvp.Key,
                    Quantity = kvp.Value
                });

                lineNo++;
            }

            return stockItems;
        }

        /// <summary>
        /// 小計前後の明細変化から、必要な仮引当または仮引当解除を実行します
        /// </summary>
        /// <param name="cart">カート Document</param>
        /// <param name="currentLineItems">小計前の明細情報</param>
        /// <param name="newLineItems">小計後の明細情報</param>
        /// <returns>仮引当処理の結果</returns>
        private async Task<bool> AdjustStocks(
            CartDocument cart,
            List<CartDocument.CartLineItem> currentLineItems,
            List<CartDocument.CartLineItem> newLineItems)
        {
            var diff = new Dictionary<string, int>();

            // 正とする新明細を辞書に登録
            foreach (var item in newLineItems)
            {
                diff.TryGetValue(item.ItemCode, out int qty);
                diff[item.ItemCode] = qty + item.Quantity;
            }

            // 参考データとしてリアルタイムで見せていた明細の在庫処理は済んでいるので減算する
            foreach (var item in currentLineItems)
            {
                diff.TryGetValue(item.ItemCode, out int qty);
                diff[item.ItemCode] = qty - item.Quantity;
            }

            // 差分を在庫サービスへ伝える
            var stockItems = new List<StockItem>();
            var lineNo = 1;
            foreach (var kvp in diff)
            {
                if (kvp.Value == 0)
                {
                    continue;
                }

                stockItems.Add(new StockItem()
                {
                    LineNo = lineNo,
                    ItemCode = kvp.Key,
                    Quantity = kvp.Value
                });

                lineNo++;
            }

            if (stockItems.Count == 0)
            {
                return true;
            }

            var stockRepo = new StocksRepository(StaticHttpClient.Instance);
            var stockResult = await stockRepo.UpdateStocksAsync(
                cart,
                stockItems,
                StockAllocationType.ProvisionalAllocation,
                this._log);
            return stockResult;
        }
    }
}
