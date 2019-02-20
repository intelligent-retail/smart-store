using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace ClientService.BoxAdminBff
{
    public static class SignalRFunction
    {
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
            [SignalRConnectionInfo(HubName = "monitor", ConnectionStringSetting = "SignalRConnection")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }
    }
}
