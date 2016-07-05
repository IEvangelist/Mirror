using Microsoft.ProjectOxford.Emotion;
using Mirror.Core;
using Mirror.Extensions;
using Mirror.Networking;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.FaceAnalysis;
using Windows.Media.MediaProperties;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace Mirror
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        bool _isStreaming;
        FaceTracker _faceTracker;
        ThreadPoolTimer _frameProcessingTimer;
        VideoEncodingProperties _videoProperties;
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

            // Enusre that our face-tracker is initialized before invoking a change of the strem-state.
            _faceTracker = await FaceTracker.CreateAsync();

            await Task.WhenAll(ChangeStreamStateAsync(true),
                               Task.CompletedTask);
        }

        async Task<bool> StartWebcamStreamingAsync()
        {
            bool successful = true;

            try
            {
                await _mediaManager.InitializeAsync(new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Video
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

                var timerInterval = TimeSpan.FromMilliseconds(66);
                _frameProcessingTimer =
                    ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(ProcessCurrentVideoFrame), timerInterval);

            }
            catch (Exception ex)
            {
                successful = false;
            }

            return successful;
        }

        async Task ShutdownWebcamAsync()
        {
            _frameProcessingTimer?.Cancel();

            if (_mediaManager.CameraStreamState == CameraStreamState.Streaming)
            {
                try
                {
                    await _mediaManager.StopPreviewAsync();
                }
                catch (Exception)
                {
                    // Since we're going to destroy the MediaCapture object there's nothing to do here
                }
            }

            _frameProcessingTimer = null;
            _preview.Source = null;
        }

        async void ProcessCurrentVideoFrame(ThreadPoolTimer timer)
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
                                                         _videoProperties.Width.ToSigned(),
                                                         _videoProperties.Height.ToSigned()))
                {
                    await _mediaManager.GetPreviewFrameAsync(previewFrame);

                    IList<DetectedFace> faces = null;

                    // The returned VideoFrame should be in the supported NV12 format but we need to verify this.
                    if (FaceDetector.IsBitmapPixelFormatSupported(previewFrame.SoftwareBitmap.BitmapPixelFormat))
                    {
                        faces = await _faceTracker.ProcessNextFrameAsync(previewFrame);
                    }

                    // Create our visualization using the frame dimensions and face results but run it on the UI thread.
                    var previewFrameSize = new Size(previewFrame.SoftwareBitmap.PixelWidth, previewFrame.SoftwareBitmap.PixelHeight);
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        SetupVisualization(previewFrameSize, faces);
                    });
                }
            }
            finally
            {
                _frameProcessingSemaphore.Release();
            }
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

                foreach (var face in foundFaces)
                {
                    var box = new Rectangle
                    {
                        Width = (uint)(face.FaceBox.Width / widthScale),
                        Height = (uint)(face.FaceBox.Height / heightScale),
                        Fill = new SolidColorBrush(Colors.Transparent),
                        Stroke = new SolidColorBrush(Colors.Azure),
                        Opacity = .5,
                        StrokeThickness = 2,
                        RadiusX = 15,
                        RadiusY = 15,
                        Margin = new Thickness((uint)(face.FaceBox.X / widthScale), (uint)(face.FaceBox.Y / heightScale), 0, 0)
                    };

                    _visualizationCanvas.Children.Add(box);
                }
            }
        }

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