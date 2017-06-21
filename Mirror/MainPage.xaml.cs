#region Using Statement(s)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MetroLog;
using Microsoft.ProjectOxford.Emotion;
using Mirror.Controls;
using Mirror.Core;
using Mirror.Emotion;
using Mirror.Extensions;
using Mirror.Interfaces;
using Mirror.IO;
using Mirror.Logging;
using Mirror.Models;
using Mirror.Speech;
using Mirror.Threading;
using Mirror.ViewModels;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using RawEmotion = Microsoft.ProjectOxford.Emotion.Contract.Emotion;

#endregion


namespace Mirror
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IContextSynthesizer
    {
        #region Field(s)
        
        ILogger _logger = LoggerFactory.Get<MainPage>();
        MediaCapture _mediaManager = new MediaCapture();
        EmotionServiceClient _emotionClient = new EmotionServiceClient(Settings.Instance.AzureEmotionApiKey);

        ISpeechEngine _speechEngine = Services.Get<ISpeechEngine>();
        IPhotoService _photoService = Services.Get<IPhotoService>();

        #endregion

        public MainPage()
        {
            InitializeComponent();
            DataContext = new HudViewModel(this);
        }

        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            _messageLabel.Text = GetTimeOfDayGreeting();

            // I want these to be serialized.
            foreach (var loader in new IAsyncLoader[]
                                   {
                                       _currentWeather,
                                       _forecastWeather,
                                       _eventCalendar
                                   }.Where(loader => loader != null))
            {
                await loader.LoadAsync();
            }

            _speechEngine.PhraseRecognized += OnSpeechEnginePhraseRecognized;
            _speechEngine.StateChanged += OnSpeechEngineStateChanged;
            _speaker.Volume = .5;

            await _speechEngine.StartContinuousRecognitionAsync();
        }

        private static string GetTimeOfDayGreeting()
        {
            var hour = DateTime.Now.Hour;
            return hour < 12
                ? "Good morning"
                : hour < 17
                    ? "Good afternoon"
                    : "Good evening";
        }

        async void OnSpeechEngineStateChanged(object sender, StateChangedEventArgs e)
            => await this.ThreadSafeAsync(() => _hypothesis.Text = e.ToString());

        async void OnSpeechEnginePhraseRecognized(object sender, PhraseRecognizedEventArgs e)
        {
            await this.ThreadSafeAsync(() => _voiceCommand.Text = e.CommandContext.Command.ToString().SplitCamelCase());

            IContextSynthesizer synthesizer = null;
            switch (e.CommandContext.Command)
            {
                case Command.CurrentWeather:
                    synthesizer = _currentWeather;
                    break;
                case Command.ForecastWeather:
                    synthesizer = _forecastWeather;
                    break;
                case Command.CalendarEvents:
                    synthesizer = _eventCalendar;
                    break;
                case Command.Audio:
                    await _speechEngine.SetRecognitionModeAsync(SpeechRecognitionMode.Paused);
                    IAudioCommandListener audioListener = _audioPlayer;
                    await audioListener.PlayRandomSongAsync();
                    break;
                case Command.Volume:
                    IVolumeCommandListener volumeListener = _audioPlayer;
                    await volumeListener.SetVolumeAsync(e.PhraseText);
                    await _speaker.SetVolumeFromCommandAsync(e.PhraseText);
                    break;
                case Command.Emotion:
                    await _speechEngine.SetRecognitionModeAsync(SpeechRecognitionMode.Paused);
                    await this.ThreadSafeAsync(async () => await ChangeStreamStateAsync(true));
                    break;
                case Command.Help:
                    synthesizer = this;
                    break;
            }

            if (synthesizer != null)
            {
                await this.ThreadSafeAsync(
                    async () =>
                        await _speechEngine.SpeakAsync(
                            await synthesizer.GetContextualMessageAsync(e.CommandContext.DateContext), _speaker));
            }
        }

        async void OnUnloaded(object sender, RoutedEventArgs e)
            => await _photoService.CleanupAsync();

        async Task<IEnumerable<RawEmotion>> CaptureEmotionAsync()
        {
            RawEmotion[] result = null;

            try
            {
                var photoFile = await _photoService.CreateAsync();
                var imageProperties = ImageEncodingProperties.CreateBmp();
                await _mediaManager.CapturePhotoToStorageFileAsync(imageProperties, photoFile);
                result = await _emotionClient.RecognizeAsync(await photoFile.OpenStreamForReadAsync());
            }
            finally
            {
                await _photoService.CleanupAsync();
            }

            return result.IsNullOrEmpty()
                ? await TaskCache<IEnumerable<RawEmotion>>.Value(() => Enumerable.Empty<RawEmotion>())
                : result;
        }

        async Task<string> GetNamedCameraOrDefault(string cameraName = "Microsoft® LifeCam HD-3000")
        {
            var videoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            return videoDevices.Where(camera =>
                                      camera.IsEnabled &&
                                      camera.Name.Equals(cameraName, StringComparison.OrdinalIgnoreCase))
                               .Select(camera => camera.Id)
                               .FirstOrDefault()
                   ?? videoDevices.Select(camera => camera.Id)
                                  .SingleOrDefault();
        }

        async Task<bool> StartWebcamStreamingAsync()
        {
            bool successful = true;

            try
            {
                if (_mediaManager != null)
                {
                    _mediaManager.Failed -= OnCameraStreamFailed;
                    _mediaManager.Dispose();
                    _mediaManager = new MediaCapture();
                }

                var cameraId = await GetNamedCameraOrDefault();
                await _mediaManager.InitializeAsync(new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Video,
                    VideoDeviceId = cameraId
                });

                _mediaManager.Failed += OnCameraStreamFailed;
                _preview.Source = _mediaManager;

                await _mediaManager.StartPreviewAsync();
                await CaptureAndProcessEmotionAsync();
            }
            catch (Exception ex) when (DebugHelper.IsHandled<MainPage>(ex))
            {
                successful = false;
            }

            return successful;
        }

        async Task ShutdownWebcamAsync()
        {
            try
            {
                if (_mediaManager.CameraStreamState == CameraStreamState.Streaming)
                {
                    await _mediaManager.StopPreviewAsync();
                }
            }
            catch (Exception ex) when (DebugHelper.IsHandled<MainPage>(ex))
            {
                // Since we're going to destroy the MediaCapture object there's nothing to do here
            }
            finally
            {
                _preview.Source = null;
            }
        }

        async Task CaptureAndProcessEmotionAsync()
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
                {
                    var emotions = await CaptureEmotionAsync();
                    var mostProbable =
                        emotions.ToResults()
                                .Where(result => result != Result.Empty)
                                .FirstOrDefault();

                    if (mostProbable != null)
                    {
                        _emoticon.Text = Emoticons.From(mostProbable.Emotion);
                        var current = _messageLabel.Text;
                        var message =
                            EmotionMessages.Messages[mostProbable.Emotion]
                                           .First(msg => msg != current);

                        _messageLabel.Text = message;

                        await ChangeStreamStateAsync(false);
                        await _speechEngine.SpeakAsync(message, _speaker);
                    }
                    else
                    {
                        await ChangeStreamStateAsync(false);
                    }
                    
                    await _speechEngine.SetRecognitionModeAsync(SpeechRecognitionMode.CommandPhrases);
                });
            }
            catch (Exception ex) when (DebugHelper.IsHandled<MainPage>(ex))
            {
            }
        }

        async Task ChangeStreamStateAsync(bool isStreaming)
        {
            switch (isStreaming)
            {
                case false:
                    await ShutdownWebcamAsync();
                    break;

                case true:
                    if (!await StartWebcamStreamingAsync())
                    {
                        await ChangeStreamStateAsync(false);
                    }
                    break;
            }
        }

        async void OnCameraStreamFailed(MediaCapture sender, object args)
            => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                    await ChangeStreamStateAsync(false));

        async Task OnTrackedChanged(object sender, Song song)
            => await this.ThreadSafeAsync(() => _songDetails.Text = song.ToString());

        Task<string> IContextSynthesizer.GetContextualMessageAsync(DateTime? dateContext)
            => Task.FromResult("You can say things like, \"what's the weather\", \"read the forecast\", or \"what is the temperature\". " +
                               "Or you could ask \"what's my calendar look like\" or \"what are my upcoming events\". " +
                               "For music, you can say \"play a song\", \"turn this up\", and other common commands like, \"mute\", etc. " +
                               "Finally, you can ask \"how do I look\"!");

        async void OnSpeakerVolumeChanged(object sender, RoutedEventArgs e)
            => await this.ThreadSafeAsync(() => _volume.Text = $"Volume: {_speaker.Volume:P0}");

        async Task OnSongEnded(object sender, bool e)
            => await _speechEngine.SetRecognitionModeAsync(SpeechRecognitionMode.CommandPhrases);
    }
}