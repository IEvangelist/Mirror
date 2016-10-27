using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using static Mirror.Extensions.StringExtensions;

namespace Mirror.IO
{
    public interface IBluetoothService
    {
        Task<DeviceInformation> PairAsync(string name);

        Task<BluetoothDevice> FromIdAsync(string id);
    }

    public class BluetoothService : IBluetoothService
    {
        async Task<DeviceInformation> IBluetoothService.PairAsync(string name)
        {
            var selector = BluetoothDevice.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(selector);

            var iPod = devices.FirstOrDefault(device => device.Name.ContainsIgnoringCase(name ?? "iPod"));
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
                var result = await device.GetRfcommServicesAsync();
                foreach (var service in result.Services)
                {
                    var attributes = await service.GetSdpRawAttributesAsync();
                    foreach (var attr in attributes)
                    {
                    }
                }
            }

            return device;
        }
    }
}