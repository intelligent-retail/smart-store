using System.Threading.Tasks;

namespace SmartRetailApp.Services
{
    public interface IQrScanningService
    {
        Task<ZXing.Result> ScanAsync();
    }
}
