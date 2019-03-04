using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ZXing.Mobile;
using Xamarin.Forms;
using SmartRetailApp.Droid.Services;
using SmartRetailApp.Services;

[assembly: Dependency(typeof(QrScanningService))]
namespace SmartRetailApp.Droid.Services
{
    public class QrScanningService : IQrScanningService
    {
        public async Task<ZXing.Result> ScanAsync()
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
    }
}