#region Using Statement(s)

using MetroLog;
using Microsoft.ProjectOxford.Emotion;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.FaceAnalysis;
using Windows.Media.MediaProperties;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
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

        bool _isStreaming;
        bool _isProcessing;
        int _captureCounter;
        const int MaxCaptureBeforeReset = 3;
        ILogger _logger = LoggerFactory.Get<MainPage>();
        FaceTracker _faceTracker;
        VideoEncodingProperties _videoProperties;
        DispatcherTimer _frameProcessingTimer;
        SemaphoreSlim _frameProcessingSemaphore = new SemaphoreSlim(1);
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
            _messageLabel.Text = "Hello";
            
            _faceTracker = await FaceTracker.CreateAsync();

            _speechEngine.PhraseRecognized += OnspeechEnginePhraseRecognized;
            _speechEngine.StateChanged += OnSpeechEngineStateChanged;

            await _speechEngine.StartContinuousRecognitionAsync();
            //await Task.WhenAll(//ChangeStreamStateAsync(true));,
            //                   InitializeSpeechRecognizerAsync());

            //var iPod = await Bluetooth.PairAsync();

            //if (iPod != null)
            //{
            //    var device = await Bluetooth.FromIdAsync(iPod.Id);
            //    if (device != null)
            //    {

            //    }
            //}
        }

        async void OnSpeechEngineStateChanged(object sender, StateChangedEventArgs e)
            => await this.ThreadSafeAsync(() => _hypothesis.Text = e.ToString());

        async void OnspeechEnginePhraseRecognized(object sender, PhraseRecognizedEventArgs e)
        {
            IContextSynthesizer synthesizer = null;
            switch (e.CommandContext.Command)
            {
                case Command.CurrentWeather:
                    synthesizer = _currentWeahther;
                    break;
                case Command.ForecastWeather:
                    synthesizer = _forecastWeather;
                    break;
                case Command.CalendarEvents:
                    synthesizer = _eventCalendar;
                    break;
                case Command.Audio:
                    IAudioCommandListener audioListener = _audioPlayer;
                    await audioListener.PlayRandomSongAsync();
                    break;
                case Command.Volume:
                    IVolumeCommandListener volumeListener = _audioPlayer;
                    await volumeListener.SetVolumeAsync(e.PhraseText);
                    break;
                case Command.Emotion:
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

        async void OnUnloaded(object sender, RoutedEventArgs e) => await _photoService.CleanupAsync();

        async Task<IEnumerable<RawEmotion>> CaptureEmotionAsync()
        {
            _isProcessing = true;

            RawEmotion[] result;

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
                _isProcessing = false;
            }

            return result.IsNullOrEmpty()
                ? await Task.FromResult(Enumerable.Empty<RawEmotion>())
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
                    _mediaManager.Dispose();
                    _mediaManager = new MediaCapture();
                }

                var cameraId = await GetNamedCameraOrDefault();
                await _mediaManager.InitializeAsync(new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Video,
                    VideoDeviceId = cameraId
                });

                // Ensure we're only ever wired to this multicast delegate once.
                _mediaManager.Failed -= OnCameraStreamFailed;
                _mediaManager.Failed += OnCameraStreamFailed;

                // Cache the media properties as we'll need them later.
                var deviceController = _mediaManager.VideoDeviceController;
                _videoProperties = deviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

                // Immediately start streaming to our CaptureElement UI.
                // NOTE: CaptureElement's Source must be set before streaming is started.
                _preview.Source = _mediaManager;
                await _mediaManager.StartPreviewAsync();

                if (_frameProcessingTimer != null)
                {
                    _frameProcessingTimer.Tick -= ProcessCurrentVideoFrame;
                    _frameProcessingTimer.Stop();
                    _frameProcessingTimer = null;
                }

                _frameProcessingTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(500)
                };
                _frameProcessingTimer.Tick += ProcessCurrentVideoFrame;
                _frameProcessingTimer.Start();
            }
            catch (Exception ex) when (DebugHelper.IsHandled<MainPage>(ex))
            {
                successful = false;
            }

            return successful;
        }

        async Task ShutdownWebcamAsync()
        {
            _frameProcessingTimer?.Stop();

            if (_mediaManager.CameraStreamState == CameraStreamState.Streaming)
            {
                try
                {
                    await _mediaManager.StopPreviewAsync();
                }
                catch (Exception ex) when (DebugHelper.IsHandled<MainPage>(ex))
                {
                    // Since we're going to destroy the MediaCapture object there's nothing to do here
                }
            }

            _frameProcessingTimer = null;
            _preview.Source = null;
        }

        async void ProcessCurrentVideoFrame(object sender, object e)
        {
            // If a lock is being held it means we're still waiting for processing work on the previous frame to complete.
            // In this situation, don't wait on the semaphore but exit immediately.
            if (!_isStreaming || !_frameProcessingSemaphore.Wait(0))
            {
                return;
            }

            try
            {
                using (var previewFrame = new VideoFrame(BitmapPixelFormat.Nv12,
                                                         (int)_videoProperties.Width,
                                                         (int)_videoProperties.Height))
                {
                    await _mediaManager.GetPreviewFrameAsync(previewFrame);

                    IList<DetectedFace> faces = null;

                    // The returned VideoFrame should be in the supported NV12 format but we need to verify this.
                    if (FaceDetector.IsBitmapPixelFormatSupported(previewFrame.SoftwareBitmap.BitmapPixelFormat))
                    {
                        faces = await _faceTracker.ProcessNextFrameAsync(previewFrame);
                    }

                    //// Create our visualization using the frame dimensions and face results but run it on the UI thread.
                    var previewFrameSize = new Size(previewFrame.SoftwareBitmap.PixelWidth, previewFrame.SoftwareBitmap.PixelHeight);
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        SetupVisualization(previewFrameSize, faces);

                        if (_isProcessing) return;

                        var emotions = await CaptureEmotionAsync();
                        if (emotions.IsNullOrEmpty() == false)
                        {
                            var mostProbable =
                                emotions.ToResults()
                                        .Where(result => result != Result.Empty)
                                        .FirstOrDefault();

                            if (mostProbable == null)
                            {
                                _messageLabel.Text = string.Empty;
                                _emoticon.Text = string.Empty;
                            }
                            else
                            {
                                _emoticon.Text = Emoticons.From(mostProbable.Emotion);

                                var current = _messageLabel.Text;
                                var message = EmotionMessages.Messages[mostProbable.Emotion].RandomElement();
                                while (current == message)
                                {
                                    message = EmotionMessages.Messages[mostProbable.Emotion].RandomElement();
                                }
                                _messageLabel.Text = message;
                                await _speechEngine.SpeakAsync(message, _speaker);

                                ++ _captureCounter;
                                if (_captureCounter >= MaxCaptureBeforeReset)
                                {
                                    await ChangeStreamStateAsync(false);
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex) when (DebugHelper.IsHandled<MainPage>(ex))
            {
            }
            finally
            {
                _frameProcessingSemaphore.Release();
            }
        }

        void SetupVisualization(Size framePixelSize, IList<DetectedFace> foundFaces)
        {
            _visualizationCanvas.Children.Clear();

            var actualWidth = _visualizationCanvas.ActualWidth;
            var actualHeight = _visualizationCanvas.ActualHeight;

            if (_isStreaming && !foundFaces.IsNullOrEmpty() && actualWidth != 0 && actualHeight != 0)
            {
                var widthScale = framePixelSize.Width / actualWidth;
                var heightScale = framePixelSize.Height / actualHeight;

                var face = GetClosestFace(foundFaces);
                if (face != null)
                {
                    var box = new Rectangle
                    {
                        Width = (uint)(face.FaceBox.Width / widthScale),
                        Height = (uint)(face.FaceBox.Height / heightScale),
                        Fill = new SolidColorBrush(Colors.Transparent),
                        Stroke = new SolidColorBrush(Colors.Azure),
                        Opacity = .25,
                        StrokeThickness = 2,
                        RadiusX = 15,
                        RadiusY = 15,
                        Margin = new Thickness((uint)(face.FaceBox.X / widthScale), (uint)(face.FaceBox.Y / heightScale), 0, 0)
                    };

                    _visualizationCanvas.Children.Add(box);
                }
            }
        }

        static DetectedFace GetClosestFace(IList<DetectedFace> faces)
            => faces?.OrderByDescending(face => face.FaceBox.Height + face.FaceBox.Width)
                     .FirstOrDefault();

        async Task ChangeStreamStateAsync(bool isStreaming)
        {
            switch (isStreaming)
            {
                case false:

                    await ShutdownWebcamAsync();

                    _visualizationCanvas.Children.Clear();
                    _isStreaming = isStreaming;
                    break;

                case true:

                    if (!await StartWebcamStreamingAsync())
                    {
                        await ChangeStreamStateAsync(false);
                        break;
                    }

                    _visualizationCanvas.Children.Clear();
                    _isStreaming = isStreaming;
                    break;
            }
        }

        /// <summary>
        /// Handles MediaCapture stream failures by shutting down streaming and returning to Idle state.
        /// </summary>
        async void OnCameraStreamFailed(MediaCapture sender, object args)
            => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                    await ChangeStreamStateAsync(false));

        async Task OnTrackedChanged(object sender, Song song) 
            => await this.ThreadSafeAsync(() => _songDetails.Text = song.ToString());

        Task<string> IContextSynthesizer.GetContextualMessageAsync(DateTime? dateContext)
            => Task.FromResult("You can say things like, \"what's the weather\", \"read the forecast\", or \"what is the temparature\". " + 
                               "Or you could ask \"what's my calendar look like\" or \"what are my upcoming events\". " + 
                               "For music, you can say \"play a song\", \"turn this up\", and other common commands like, \"mute\", etc. " +
                               "Finally, you can ask \"how do I look\"!");
    }
}