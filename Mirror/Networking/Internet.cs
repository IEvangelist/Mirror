using System;
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

        public ConnectionStatus(ConnectionType type,
                                byte? signalBars= null)
        {
            Type = type;
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
                    if (profile.IsWlanConnectionProfile)
                    {
                        ConnectionChanged?.Invoke(new ConnectionStatus(ConnectionType.Wifi, profile.GetSignalBars()));
                    }
                    else if (profile.IsWwanConnectionProfile)
                    {
                        ConnectionChanged?.Invoke(new ConnectionStatus(ConnectionType.Cellular, profile.GetSignalBars()));
                    }
                    else
                    {
                        ConnectionChanged?.Invoke(new ConnectionStatus(ConnectionType.Ethernet));
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