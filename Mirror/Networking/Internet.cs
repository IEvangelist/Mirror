using System;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;


namespace Mirror.Networking
{
    internal enum ConnectionType
    {
        None,
        Ethernet,
        Wifi,
        Cellular
    };
    
    struct ConnectionStatus
    {
        internal ConnectionType Type { get; }
        internal byte? SignalBars { get; }
        internal bool IsConnectionAvailable => Type != default(ConnectionType);

        internal ConnectionStatus(ConnectionType type,
                                  byte? signalBars= null)
        {
            Type = type;
            SignalBars = signalBars;
        }
    }

    static class Internet
    {
        internal static Func<ConnectionStatus, Task> ConnectionChanged;

        static Internet()
        {
            NetworkInformation.NetworkStatusChanged += OnNetworkStatusChanged;
            CheckInternetAvailability();
        }

        static void OnNetworkStatusChanged(object sender) => CheckInternetAvailability();

        internal static void Initialize() => CheckInternetAvailability();

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