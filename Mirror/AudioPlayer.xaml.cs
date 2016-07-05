using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Mirror
{
    public sealed partial class AudioPlayer : UserControl
    {
        public AudioPlayer()
        {
            InitializeComponent();
        }

        async void OnLoaded(object sender, RoutedEventArgs e) => await LoadAudioAsync();

        async Task LoadAudioAsync()
        {
            // TODO: Actually load 
            await Task.CompletedTask;
        }
    }
}