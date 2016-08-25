using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace Mirror.IO
{
    public interface IBluetoothService
    {
        Task<DeviceInformation> PairAsync();

        Task<BluetoothDevice> FromIdAsync(string id);
    }

    public class BluetoothService : IBluetoothService
    {
        async Task<DeviceInformation> IBluetoothService.PairAsync()
        {
            var selector = BluetoothDevice.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(selector);

            var iPod = devices.FirstOrDefault(device => device.Name.IndexOf("iPod", StringComparison.OrdinalIgnoreCase) > -1);
            if (iPod != null)
            {
                await iPod.Pairing.PairAsync(DevicePairingProtectionLevel.None);
            }

            return iPod;
        }

        async Task<BluetoothDevice> IBluetoothService.FromIdAsync(string id)
        {
            var device = await BluetoothDevice.FromIdAsync(id);
            if (device != null)
            {
                
            }

            return device;
        }
    }
}