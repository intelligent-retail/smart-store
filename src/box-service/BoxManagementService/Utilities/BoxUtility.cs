using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace BoxManagementService.Utilities
{
    public static class BoxUtility
    {
        public static async Task RequestUnLockAsync(string boxId, string deviceId)
        {
            var svClient = ServiceClient.CreateFromConnectionString(Settings.Instance.IotHubConnectionString);

            var parameter =
                new
                {
                    box_id = boxId,
                    Init = 0,
                    LightON = 1,
                    DoorOpen = 1,
                    DoorLock = 1,
                    device_type = "SmartBox",
                    command = "1",
                    transactionId = "0"
                };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(parameter)));

            await svClient.SendAsync(deviceId, msg);
        }
    }
}
