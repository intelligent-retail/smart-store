using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PosService.Constants;
using PosService.Models.Documents;
using Microsoft.Azure.Documents.Client;

namespace PosService.Models.Repositories.CosmosDB
{
    /// <summary>
    /// レシートリポジトリ
    /// </summary>
    public class ReceiptRepository : AbstractRepository<ReceiptDocument>
    {
        /// <summary>Cosmos DB の ID</summary>
        private static readonly string ReceiptsDatabaseId = "smartretailpos";

        /// <summary>Cosmos DB の コレクション ID</summary>
        private static readonly string ReceiptsCollectionId = "Receipts";

        /// <summary>レシート幅</summary>
        private readonly int _receiptWith = 32;

        /// <summary>エンコード</summary>
        private readonly string _encodeName = "Shift_JIS";

        /// <summary>作成中のレシートデータ</summary>
        private readonly StringBuilder _receiptData = new StringBuilder();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="documentClient">Cosmos DB クライアント</param>
        public ReceiptRepository(DocumentClient documentClient)
            : base(documentClient, ReceiptsDatabaseId, ReceiptsCollectionId)
        {
        }

        /// <summary>
        /// 取引ログからレシートを作成・登録します
        /// </summary>
        /// <param name="cartDoc">カート Document</param>
        /// <returns>作成したレシート Document</returns>
        public async Task<ReceiptDocument> CreateReceiptAsync(CartDocument cartDoc)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var receiptDoc = new ReceiptDocument()
            {
                CompanyCode = cartDoc.CompanyCode,
                StoreCode = cartDoc.StoreCode,
                TerminalNo = cartDoc.TerminalNo,
                TransactionNo = cartDoc.TransactionNo,
                TransactionType = cartDoc.TransactionType,
                BusinessDate = cartDoc.BusinessDate,
                GenerateDateTime = cartDoc.GenerateDateTime,
                ReceiptNo = cartDoc.ReceiptNo,
                ReceiptText = this.MakeReceiptLayout(cartDoc),
                User = new UserInfo { UserId = cartDoc.User.UserId, UserName = cartDoc.User.UserName }
            };

            await this.CreateAsync(receiptDoc);
            return receiptDoc;
        }

        /// <summary>
        /// レシートをレイアウトします
        /// </summary>
        /// <param name="cartDoc">カート Document</param>
        /// <returns>レイアウトしたレシート</returns>
        private string MakeReceiptLayout(CartDocument cartDoc)
        {
            // ヘッダ作成
            var title = cartDoc.TransactionType == TransactionType.NormalSales.TypeNo ?
                "            領 収 書            " : "        取引中止＜売上＞        ";
            this._receiptData.AppendLine(title);
            this.MakeText($"店No：{cartDoc.StoreCode}", $"端末No：{cartDoc.TerminalNo.ToString().PadLeft(4, '0')}");
            this._receiptData.AppendLine(cartDoc.GenerateDateTime.ToString("yyyy年MM月dd日 HH時mm分"));
            this._receiptData.AppendLine(string.Empty);
            this.MakeCenterText(cartDoc.StoreName);
            this.MakeCenterText("TEL:03-XXXX-XXXX");
            this._receiptData.AppendLine("お買い上げありがとうございます。");
            this._receiptData.AppendLine(string.Empty);

            // 明細作成
            cartDoc.LineItems.ForEach(item =>
            {
                string amountString = string.Empty;
                string unitPriceString = string.Empty;
                string qtyString = string.Empty;

                amountString = this.MakeCurrencyText(item.Amount);
                this.MakeText(item.DescriptionShort, amountString + " ");

                // 単価ｘ個数
                if (item.Quantity > 1)
                {
                    unitPriceString = "@" + this.MakeCurrencyText(item.UnitPrice);
                    unitPriceString = unitPriceString.PadLeft(11, ' ');
                    qtyString = "x" + item.Quantity.ToString("#,0");
                    this._receiptData.AppendLine(unitPriceString + qtyString);
                }
            });

            this._receiptData.AppendLine(string.Empty.PadLeft(this._receiptWith, '-'));

            // 小計・支払作成
            this.MakeText("買上点数", cartDoc.Sales.TotalQuantity.ToString("#,0") + " ");
            this.MakeText("小計", this.MakeCurrencyText(cartDoc.Sales.TotalAmount) + " ");

            cartDoc.Taxes.ForEach(tax =>
            {
                var taxInfo = cartDoc.Masters.Taxes.Where(row => row.TaxCode == tax.TaxCode).FirstOrDefault();

                if (taxInfo.TaxCode == TaxType.IncludedTax.Code)
                {
                    string includedTaxAmountString = this.MakeCurrencyText(tax.TaxAmount) + ")";
                    decimal includedTaxRate = taxInfo.Rate / 100;
                    this.MakeText("(内税額" + includedTaxRate.ToString("P0").PadLeft(3, ' '), includedTaxAmountString);
                }
                else
                {
                    throw new NotImplementedException();
                }
            });

            this._receiptData.AppendLine(string.Empty);

            this.MakeText("税込合計", this.MakeCurrencyText(cartDoc.Sales.TotalAmountWithTaxes) + " ");

            cartDoc.Payments.ForEach(payment =>
            {
                this.MakeText(payment.Description, this.MakeCurrencyText(payment.Amount) + " ");
            });

            this.MakeText("お釣り", this.MakeCurrencyText(cartDoc.Sales.ChangeAmount) + " ");

            // フッタ
            this._receiptData.AppendLine(string.Empty);
            this._receiptData.AppendLine("上記正に領収いたしました。");
            this._receiptData.AppendLine(string.Empty);

            var receiptNoString = cartDoc.ReceiptNo != 0 ? cartDoc.ReceiptNo.ToString().Trim().PadLeft(4, '0') : "----";
            this._receiptData.AppendLine("レシートNo" + receiptNoString);

            return this._receiptData.ToString();
        }

        /// <summary>
        /// 中央に空白を挟んだ文字列を作成します
        /// </summary>
        /// <param name="leftText">左詰め文字列</param>
        /// <param name="rightText">右詰め文字列</param>
        /// <returns>結果文字列</returns>
        private void MakeText(string leftText, string rightText)
        {
            var spaceCount = this._receiptWith - Encoding.GetEncoding(this._encodeName).GetByteCount(leftText) - Encoding.GetEncoding(this._encodeName).GetByteCount(rightText);

            this._receiptData.AppendLine(
                leftText +
                (spaceCount > 0 ? string.Empty.PadLeft(spaceCount, ' ') : string.Empty) +
                rightText);
        }

        /// <summary>
        /// 中央寄せの文字列を作成します
        /// </summary>
        /// <param name="text">対象文字列</param>
        /// <returns>結果文字列</returns>
        private void MakeCenterText(string text)
        {
            var spaceCount = this._receiptWith - Encoding.GetEncoding(this._encodeName).GetByteCount(text) / 2;

            this._receiptData.AppendLine(
                text.PadLeft(spaceCount > 0 ? spaceCount : 0, ' '));
        }

        private string MakeCurrencyText(decimal amount)
            => amount.ToString("C", new CultureInfo("ja-JP"));
    }
}
