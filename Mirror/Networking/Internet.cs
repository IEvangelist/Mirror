using System;
using System.Linq;
using Windows.Networking;
using Windows.Networking.Connectivity;


namespace Mirror.Networking
{
    public enum ConnectionType
    {
        None,
        Ethernet,
        Wifi,
        Cellular
    };
    
    struct ConnectionStatus
    {
        public ConnectionType Type { get; }
        public byte? SignalBars { get; }
        public bool IsConnectionAvailable => Type != default(ConnectionType);
        public string IpAddress { get; }

        public ConnectionStatus(ConnectionType type,
                                string ipAddress,
                                byte? signalBars = null)
        {
            Type = type;
            IpAddress = ipAddress;
            SignalBars = signalBars;
        }
    }

    static class Internet
    {
        public static Action<ConnectionStatus> ConnectionChanged;

        static Internet()
        {
            NetworkInformation.NetworkStatusChanged += OnNetworkStatusChanged;
            CheckInternetAvailability();
        }

        static void OnNetworkStatusChanged(object sender) => CheckInternetAvailability();

        public static void Initialize() => CheckInternetAvailability();

        static void CheckInternetAvailability()
        {
            TryUseProfile(profile =>
            {
                if (profile?.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess)
                {
                    var ipHost = 
                        NetworkInformation.GetHostNames()
                                          .FirstOrDefault(host => 
                                                          host.IPInformation != null && 
                                                          host.Type == HostNameType.Ipv4);
                    var ipAddress = ipHost?.ToString() ?? "0.0.0.1";
                    if (profile.IsWlanConnectionProfile)
                    {
                        ConnectionChanged?.Invoke(new ConnectionStatus(ConnectionType.Wifi, ipAddress, profile.GetSignalBars()));
                    }
                    else if (profile.IsWwanConnectionProfile)
                    {
                        ConnectionChanged?.Invoke(new ConnectionStatus(ConnectionType.Cellular, ipAddress, profile.GetSignalBars()));
                    }
                    else
                    {
                        ConnectionChanged?.Invoke(new ConnectionStatus(ConnectionType.Ethernet, ipAddress));
                    }
                }
                else
                {
                    ConnectionChanged?.Invoke(default(ConnectionStatus));
                }
            });
        }

        static void TryUseProfile(Action<ConnectionProfile> useProfile) =>
            useProfile?.Invoke(NetworkInformation.GetInternetConnectionProfile());
    }
}