using Microsoft.ProjectOxford.Emotion;
using Mirror.Core;
using Mirror.Emotion;
using Mirror.Extensions;
using Mirror.IO;
using Mirror.Models;
using Mirror.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using RawEmotion = Microsoft.ProjectOxford.Emotion.Contract.Emotion;


namespace Mirror
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        bool _isStreaming;
        bool _isProcessing;
        FaceTracker _faceTracker;
        SpeechRecognizer _speechRecognizer;
        SpeechSynthesizer _speechSynthesizer;
        DispatcherTimer _frameProcessingTimer;
        VideoEncodingProperties _videoProperties;
        StringBuilder _dictatedTextBuilder = new StringBuilder();
        SemaphoreSlim _frameProcessingSemaphore = new SemaphoreSlim(1);
        MediaCapture _mediaManager = new MediaCapture();
        EmotionServiceClient _emotionClient = new EmotionServiceClient(Settings.Instance.AzureEmotionApiKey);

        public MainPage()
        {
            InitializeComponent();
        }

        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitializeInternet();

            _messageLabel.Text = "Hello";

            // Enusre that our face-tracker is initialized before invoking a change of the strem-state.
            _faceTracker = await FaceTracker.CreateAsync();

            _speechSynthesizer = new SpeechSynthesizer();
            _speechSynthesizer.Voice =
                SpeechSynthesizer.AllVoices
                                 .FirstOrDefault(voice =>
                                                 voice.Gender == VoiceGender.Female &&
                                                 voice.Language.Contains("en-US"));

            await Task.WhenAll(ChangeStreamStateAsync(true),
                               InitializeSpeechRecognizerAsync());
        }
        private async Task InitializeSpeechRecognizerAsync()
        {
            if (_speechRecognizer != null)
            {
                _speechRecognizer.StateChanged -= OnSpeechRecognizerStateChanged;
                _speechRecognizer.ContinuousRecognitionSession.Completed -= OnContinuousRecognitionSessionCompleted;
                _speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= OnContinuousRecognitionSessionResultGenerated;
                _speechRecognizer.HypothesisGenerated -= OnSpeechRecognitionHypothesisGenerated;
                _speechRecognizer.Dispose();
                _speechRecognizer = null;
            }

            _speechRecognizer = new SpeechRecognizer();
            _speechRecognizer.StateChanged += OnSpeechRecognizerStateChanged;            
            _speechRecognizer.Constraints
                             .Add(new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, 
                                                                       "dictation"));

            var result = await _speechRecognizer.CompileConstraintsAsync();

            _speechRecognizer.ContinuousRecognitionSession.Completed += OnContinuousRecognitionSessionCompleted;
            _speechRecognizer.ContinuousRecognitionSession.ResultGenerated += OnContinuousRecognitionSessionResultGenerated;
            _speechRecognizer.HypothesisGenerated += OnSpeechRecognitionHypothesisGenerated;

            await _speechRecognizer.ContinuousRecognitionSession.StartAsync();
        }

        private asc void OnSpeechRecognizerStateChanged(
            SpeechRecognizer sender,
            SpeechRecognizerStateChangedEventArgs args)
        {
            //await this.ThreadSafeAsync(() => _hypothesis.Text = args.State.ToString());
            switch (args.State)
            {
                case SpeechRecognizerState.Idle:                    
                    break;
                case SpeechRecognizerState.Capturing:
                    break;
                case SpeechRecognizerState.Processing:
                    break;
                case SpeechRecognizerState.SoundStarted:
                    break;
                case SpeechRecognizerState.SoundEnded:
                    break;
                case SpeechRecognizerState.SpeechDetected:
                    break;
                case SpeechRecognizerState.Paused:
                    break;
            }
        }

        private async void OnContinuousRecognitionSessionCompleted(
            SpeechContinuousRecognitionSession sender,
            SpeechContinuousRecognitionCompletedEventArgs args)
        {
            // await _speechRecognizer.ContinuousRecognitionSession.StopAsync();
            await InitializeSpeechRecognizerAsync();
        }

        private async void OnContinuousRecognitionSessionResultGenerated(
            SpeechContinuousRecognitionSession sender,
            SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            // We may choose to discard content that has low confidence, as that could indicate that we're picking up
            // noise via the microphone, or someone could be talking out of earshot.
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
                args.Result.Confidence == SpeechRecognitionConfidence.High)
            {
                _dictatedTextBuilder.Append(args.Result.Text + " ");

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    _hypothesis.Text = _dictatedTextBuilder.ToString();
                });
            }
        }

        private async void OnSpeechRecognitionHypothesisGenerated(
            SpeechRecognizer sender, 
            SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            string hypothesis = args.Hypothesis.Text;            
            string hypothesisText = $"{_dictatedTextBuilder} {hypothesis} ...";

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _hypothesis.Text = hypothesisText;
            });
        }

        async void OnUnloaded(object sender, RoutedEventArgs e) => await Photos.CleanupAsync();

        async Task<IEnumerable<RawEmotion>> CaptureEmotionAsync()
        {
            _isProcessing = true;

            RawEmotion[] result;
            
            try
            {
                var photoFile = await Photos.CreateAsync();
                var imageProperties = ImageEncodingProperties.CreateBmp();
                await _mediaManager.CapturePhotoToStorageFileAsync(imageProperties, photoFile);
                result = await _emotionClient.RecognizeAsync(await photoFile.OpenStreamForReadAsync());
            }
            finally
            {
                await Photos.CleanupAsync();
                _isProcessing = false;
            }

            return result.IsNullOrEmpty()
                ? await Task.FromResult(Enumerable.Empty<RawEmotion>())
                : result;
        }

        async Task<string> GetNamedCameraOrDefault(string cameraName = "Microsoft® LifeCam HD-3000")
        {
            var videoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            return videoDevices.Where(cam =>
                                      cam.IsEnabled &&
                                      cam.Name.Equals(cameraName, StringComparison.OrdinalIgnoreCase))
                               .Select(cam => cam.Id)
                               .FirstOrDefault()
                   ?? videoDevices.Select(cam => cam.Id)
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
            catch (Exception ex) when (DebugHelper.IsNotHandled(ex))
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
                catch (Exception ex) when (DebugHelper.IsNotHandled(ex))
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
                                await ReadMessageAsync(message);
                            }
                        }
                    });
                }
            }
            catch (Exception ex) when (DebugHelper.IsNotHandled(ex))
            {
            }
            finally
            {
                _frameProcessingSemaphore.Release();
            }
        }

        private async Task ReadMessageAsync(string message)
        {            
            var stream = await _speechSynthesizer.SynthesizeTextToStreamAsync(message);

            _speaker.SetSource(stream, stream.ContentType);
            _speaker.Play();
        }

        void SetupVisualization(Size framePixelSize, IList<DetectedFace> foundFaces)
        {
            _visualizationCanvas.Children.Clear();

            double actualWidth = _visualizationCanvas.ActualWidth;
            double actualHeight = _visualizationCanvas.ActualHeight;

            if (_isStreaming && !foundFaces.IsNullOrEmpty() && actualWidth != 0 && actualHeight != 0)
            {
                double widthScale = framePixelSize.Width / actualWidth;
                double heightScale = framePixelSize.Height / actualHeight;

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
        /// <param name="sender">The source of the event, i.e. our MediaCapture object</param>
        /// <param name="args">Event data</param>
        void OnCameraStreamFailed(MediaCapture sender, object args)
        {
            // MediaCapture is not Agile and so we cannot invoke its methods on this caller's thread
            // and instead need to schedule the state change on the UI thread.
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await ChangeStreamStateAsync(false);
            });
        }

        void InitializeInternet()
        {
            Internet.ConnectionChanged = OnConnectionChanged;
            Internet.Initialize();
        }

        async Task OnConnectionChanged(ConnectionStatus connection)
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

        void OnTrackedChanged(object sender, Song song) => _songDetails.Text = song.ToString();
    }
}