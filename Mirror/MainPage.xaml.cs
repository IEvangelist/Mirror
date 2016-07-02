using Microsoft.ProjectOxford.Emotion;
using Mirror.Extensions;
using Mirror.Networking;
using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;


namespace Mirror
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaCapture _mediaManager = new MediaCapture();
        EmotionServiceClient _emotionClient = new EmotionServiceClient("");

        public MainPage()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitializeInternet();

            await Task.WhenAll(// InitializeCameraAsync(),
                               // CalendarClient.GetCalendarsAsync(),
                               Task.CompletedTask);
        }

        //async Task InitializeCameraAsync()
        //{
        //    await _mediaManager.InitializeAsync();

        //    _preview.Source = _mediaManager;
        //    await _mediaManager.StartPreviewAsync();
        //}

        void InitializeInternet()
        {
            Internet.ConnectionChanged = OnConnectionChanged;
            Internet.Initialize();
        }

        async void OnConnectionChanged(ConnectionStatus connection)
        {
            await this.ThreadSafeAsync(() =>
            {
                switch (connection.Type)
                {
                    case ConnectionType.Ethernet:
                        _connectionImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/ethernet.png"));
                        break;
                    case ConnectionType.Wifi:
                        string wifi = "wifi";
                        var bars = connection.SignalBars.GetValueOrDefault();
                        if (bars > 0 && bars < 4) wifi += $"-{bars}";
                        _connectionImage.Source = new BitmapImage(new Uri($"ms-appx:///Assets/{wifi}.png"));
                        break;

                    case ConnectionType.None:
                    case ConnectionType.Cellular:
                    default:
                        _connectionImage.Source = null;
                        break;
                }
            });
        }
    }
}