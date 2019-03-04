using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SmartRetailApp.iOS.Services;
using SmartRetailApp.Services;
using ZXing.Mobile;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(QrScanningService))]
namespace SmartRetailApp.iOS.Services
{
    public class QrScanningService : IQrScanningService
    {
        public async Task<ZXing.Result> ScanAsync()
        {
            try
            {
                var optionsDefault = new MobileBarcodeScanningOptions();
                var optionsCustom = new MobileBarcodeScanningOptions();

                var scanner = new MobileBarcodeScanner()
                {
                    TopText = "MS Smart Retailにようこそ",
                    BottomText = "ボックスのQRコードを読み取ってください。",
                };

                var scanResult = await scanner.Scan(optionsCustom);
                return scanResult;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
