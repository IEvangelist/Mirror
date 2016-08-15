using Mirror.Networking;
using System;
using System.ComponentModel;


namespace Mirror.ViewModels
{
    class HudViewModel : INotifyPropertyChanged
    {
        Uri _connectivitySource = new Uri($"ms-appx:///Assets/wifi.png");

        public event PropertyChangedEventHandler PropertyChanged;

        public Uri ConnectivitySource
        {
            get { return _connectivitySource; }
            set
            {
                _connectivitySource = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConnectivitySource)));
            }
        }

        internal HudViewModel()
        {
            Internet.ConnectionChanged = OnConnectionChanged;
            Internet.Initialize();
        }        

        void OnConnectionChanged(ConnectionStatus connection)
        {
            switch (connection.Type)
            {
                case ConnectionType.Ethernet:
                    ConnectivitySource = new Uri("ms-appx:///Assets/ethernet.png");
                    break;
                case ConnectionType.Wifi:
                    string wifi = "wifi";
                    var bars = connection.SignalBars.GetValueOrDefault();
                    if (bars > 0 && bars < 4) wifi += $"-{bars}";
                    ConnectivitySource = new Uri($"ms-appx:///Assets/{wifi}.png");
                    break;

                case ConnectionType.None:
                case ConnectionType.Cellular:
                default:
                    ConnectivitySource = null;
                    break;
            }
        }
    }
}