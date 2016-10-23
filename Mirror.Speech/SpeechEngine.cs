using MetroLog;
using Mirror.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Media.Capture;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Mirror.Speech
{
    public class SpeechEngine : ISpeechEngine
    {
        #region Properties, Events and Fields

        ISpeechEngine _ => this;

        const uint RecognizerNotFoundHResult = 0x8004503a;

        ILogger _logger = LoggerFactory.Get<SpeechEngine>();
        ICommandInterpreter _intentInterpreter = new CommandInterpreter();

        SpeechRecognizer _speechRecognizer;
        ResourceContext _speechContext;
        ResourceMap _speechResourceMap;
        SemaphoreSlim _mutex;
        bool _isInRecognitionSession;
        SpeechSynthesizer _speechSynthesizer;
        MediaElement _mediaElement;
        SemaphoreSlim _semaphore;

        public event EventHandler<PhraseRecognizedEventArgs> PhraseRecognized;

        public event EventHandler<StateChangedEventArgs> StateChanged;

        public SpeechRecognitionMode RecognitionMode { get; private set; }

        SpeechRecognizer SpeechRecognizer => _speechRecognizer ?? InitializeRecognizer();

        ResourceMap SpeechResourceMap => _speechResourceMap ?? (_speechResourceMap = ResourceManager.Current
                                                                                                    .MainResourceMap
                                                                                                    .GetSubtree("SpeechResources"));

        List<string> AvailablePhrases { get; set; }

        SemaphoreSlim Mutex => _mutex ?? (_mutex = new SemaphoreSlim(1));

        SpeechSynthesizer SpeechSynthesizer => _speechSynthesizer ?? (_speechSynthesizer = new SpeechSynthesizer
        {
            Voice = SpeechSynthesizer.AllVoices.FirstOrDefault(voice => voice.Gender == VoiceGender.Female) ?? SpeechSynthesizer.DefaultVoice
        });

        SemaphoreSlim Semaphore => _semaphore ?? (_semaphore = new SemaphoreSlim(0, 1));

        WaitHandle WaitHandle { get; set; }

        MediaElement MediaPlayerElement
        {
            get { return _mediaElement; }
            set
            {
                if (_mediaElement != value)
                {
                    if (_mediaElement != null)
                    {
                        _mediaElement.MediaEnded -= OnMediaElementMediaEnded;
                    }

                    _mediaElement = value;
                    _mediaElement.MediaEnded += OnMediaElementMediaEnded;
                }
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeechEngine"/> class. 
        /// </summary>
        public SpeechEngine()
        {
        }

        SpeechRecognizer InitializeRecognizer()
        {
            try
            {
                //var language = SpeechRecognizer.SystemSpeechLanguage;

                _speechContext = ResourceContext.GetForCurrentView();
                //_speechContext.Languages = new string[] { language.LanguageTag };

                _speechRecognizer = new SpeechRecognizer();
                _speechRecognizer.StateChanged += OnSpeechRecognizerStateChanged;
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == RecognizerNotFoundHResult)
                {
                    Debug.WriteLine("SpeechManager: The speech language pack for selected language isn't installed.");
                }
                else
                {
                    Debug.WriteLine(ex.ToString());
                }
            }

            return _speechRecognizer;
        }

        async Task ISpeechEngine.SetRecognitionModeAsync(SpeechRecognitionMode mode)
        {
            if (mode != RecognitionMode)
            {
                RecognitionMode = mode;

                await 
                (
                    mode == SpeechRecognitionMode.Paused
                        ? _.EndRecognitionSessionAsync()
                        : _.StartContinuousRecognitionAsync()
                );
            }
        }

        async Task ISpeechEngine.StartContinuousRecognitionAsync()
        {
            // Compiling a new grammar is potentially a high-latency operation,
            // and it's easy for various threads to call this method concurrently,
            // so use a sempahore to serialize access to this method. The semaphore
            // allows only one thread at a time to execute this code path.
            await Mutex.WaitAsync();
            await _.EndRecognitionSessionAsync();

            try
            {
                if (!await IsMicrophoneAvailableAsync())
                {
                    return;
                }
                
                await CompileGrammarAsync();
                
                SpeechRecognizer.ContinuousRecognitionSession.Completed += OnContinuousRecognitionSessionCompleted;
                SpeechRecognizer.ContinuousRecognitionSession.ResultGenerated += OnContinuousRecognitionSessionResultGenerated;

                await SpeechRecognizer.ContinuousRecognitionSession.StartAsync();

                _isInRecognitionSession = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SpeechManager: Failed to start continuous recognition session. ERROR:: " + ex.Message);
            }
            finally
            {
                Mutex.Release();
            }
        }

        async Task ISpeechEngine.SpeakAsync(string phrase, MediaElement media)
        {
            if (!string.IsNullOrEmpty(phrase))
            {
                bool isPauseRquired = RecognitionMode != SpeechRecognitionMode.Paused;

                // Turn off speech recognition while speech synthesis is happening.
                if (isPauseRquired)
                {
                    await _.SetRecognitionModeAsync(SpeechRecognitionMode.Paused);
                }

                MediaPlayerElement = media;
                var stream = await SpeechSynthesizer.SynthesizeTextToStreamAsync(phrase);

                // The Play call starts the sound stream playback and immediately returns,
                // so a semaphore is required to make the SpeakAsync method awaitable.
                media.AutoPlay = true;
                media.SetSource(stream, stream.ContentType);
                media.Play();

                // Wait until the MediaEnded event on MediaElement is raised,
                // before turning on speech recognition again. The semaphore
                // is signaled in the OnMediaElementMediaEnded event handler.
                await Semaphore.WaitAsync();

                // Turn on speech recognition and listen for commands.
                if (isPauseRquired)
                {
                    await _.SetRecognitionModeAsync(SpeechRecognitionMode.CommandPhrases);
                }
            }
        }

        protected virtual void OnPhraseRecognized(PhraseRecognizedEventArgs e) => PhraseRecognized?.Invoke(this, e);

        protected virtual void OnStateChanged(StateChangedEventArgs e) => StateChanged?.Invoke(this, e);

        async Task<bool> IsMicrophoneAvailableAsync()
        {
            bool isMicrophoneAvailable = false;

            try
            {
                using (var captureDevice = new MediaCapture())
                {
                    await captureDevice.InitializeAsync();

                    // Throws if no device is available.
                    var audioDevice = captureDevice.AudioDeviceController;
                    if (audioDevice != null)
                    {
                        isMicrophoneAvailable = true;
                    }
                    else
                    {
                        Debug.WriteLine("SpeechManager: No AudioDeviceController found");
                    }
                }                    
            }
            catch (COMException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return isMicrophoneAvailable;
        }

        void PopulatePhrases()
        {
            AvailablePhrases = new List<string>();
            
            // General
            AvailablePhrases.Add(FromResource(Phrases.Help));
            AvailablePhrases.Add(FromResource(Phrases.WhatCanISay));

            // Volume
            var volumeFormat = FromResource(Phrases.VolumeAtPercent);
            AvailablePhrases.AddRange(Enumerable.Range(0, 101).Select(i => string.Format(volumeFormat, i)));
            AvailablePhrases.Add(FromResource(Phrases.TurnItDown));
            AvailablePhrases.Add(FromResource(Phrases.TurnThisDown));
            AvailablePhrases.Add(FromResource(Phrases.Louder));
            AvailablePhrases.Add(FromResource(Phrases.Quieter));
            AvailablePhrases.Add(FromResource(Phrases.TurnItUp));
            AvailablePhrases.Add(FromResource(Phrases.TurnThisUp));
            AvailablePhrases.Add(FromResource(Phrases.Mute));
            AvailablePhrases.Add(FromResource(Phrases.MuteIt));
            AvailablePhrases.Add(FromResource(Phrases.MuteThis));
            AvailablePhrases.Add(FromResource(Phrases.Unmute));
            AvailablePhrases.Add(FromResource(Phrases.UnmuteIt));
            AvailablePhrases.Add(FromResource(Phrases.UnmuteThis));

            // Emotion
            AvailablePhrases.Add(FromResource(Phrases.HowDoIFeel));
            AvailablePhrases.Add(FromResource(Phrases.HowDoILook));
            AvailablePhrases.Add(FromResource(Phrases.LookAtMe));

            // Calendar
            var days = Enum.GetNames(typeof(DayOfWeek));
            AvailablePhrases.AddRange(days.Select(day => string.Format(FromResource(Phrases.HowDoesMyDayLook), day)));
            AvailablePhrases.AddRange(days.Select(day => string.Format(FromResource(Phrases.HowIsMyDayLooking), day)));
            AvailablePhrases.AddRange(days.Select(day => string.Format(FromResource(Phrases.HowIsMyDayShapingUp), day)));
            AvailablePhrases.AddRange(days.Select(day => string.Format(FromResource(Phrases.WhatsMyCalendarOnDay), day)));
            AvailablePhrases.Add(FromResource(Phrases.HowDoesMyCalendarLook));
            AvailablePhrases.Add(FromResource(Phrases.WhatDoesMyCalendarLookLike));
            AvailablePhrases.Add(FromResource(Phrases.ReadMyUpcomingCalendar));
            AvailablePhrases.Add(FromResource(Phrases.ReadMyCalendar));
            AvailablePhrases.Add(FromResource(Phrases.ReadMyEvents));
            AvailablePhrases.Add(FromResource(Phrases.ReadMyUpcomingEvents));
            AvailablePhrases.Add(FromResource(Phrases.WhatAreMyEvents));
            AvailablePhrases.Add(FromResource(Phrases.WhatAreMyUpcomingEvents));

            // Weather
            AvailablePhrases.Add(FromResource(Phrases.ReadTheTemp));
            AvailablePhrases.Add(FromResource(Phrases.WhatIsTheTemp));
            AvailablePhrases.Add(FromResource(Phrases.ReadTheCurrentWeather));
            AvailablePhrases.Add(FromResource(Phrases.ReadTheWeather));
            AvailablePhrases.Add(FromResource(Phrases.WhatIsTheCurrentWeather));
            AvailablePhrases.Add(FromResource(Phrases.WhatIsTheWeather));

            // Forecast
            AvailablePhrases.AddRange(days.Select(day => string.Format(FromResource(Phrases.OnDayWhatIstheWeather), day)));
            AvailablePhrases.AddRange(days.Select(day => string.Format(FromResource(Phrases.OnDayWhatsTheWeather), day)));
            AvailablePhrases.AddRange(days.Select(day => string.Format(FromResource(Phrases.WhatIsTheWeatherOnDay), day)));
            AvailablePhrases.AddRange(days.Select(day => string.Format(FromResource(Phrases.WhatsTheWeatherOnDay), day)));
            AvailablePhrases.Add(FromResource(Phrases.ReadTheForecast));
            AvailablePhrases.Add(FromResource(Phrases.WhatIsTheForecast));

            // Audio
            var playFormat = FromResource(Phrases.Play);
            AvailablePhrases.AddRange(new[] { "Crosses" }.Select(band => string.Format(playFormat, band)));
            AvailablePhrases.Add(FromResource(Phrases.PlaySong));
            AvailablePhrases.Add(FromResource(Phrases.PlayMusic));
            AvailablePhrases.Add(FromResource(Phrases.PlayASong));
            AvailablePhrases.Add(FromResource(Phrases.PlayAnySong));
        }

        string FromResource(string resourceKey) 
            => SpeechResourceMap.GetValue(resourceKey, _speechContext).ValueAsString;

        async Task CompileGrammarAsync()
        {
            if (RecognitionMode == SpeechRecognitionMode.Dictation)
            {
                await CompileDictationConstraint();
            }
            else
            {
                await CompilePhraseConstraints();
            }
        }

        async Task CompilePhrasesAsync()
        {
            try
            {
                SpeechRecognizer.Constraints.Clear();

                AvailablePhrases.ForEach(p =>
                {
                    var phraseNoSpaces = p.Replace(" ", string.Empty);
                    SpeechRecognizer.Constraints.Add(
                        new SpeechRecognitionListConstraint(
                            new List<string>() { p },
                            phraseNoSpaces));
                });

                var result = await SpeechRecognizer.CompileConstraintsAsync();
                if (result.Status != SpeechRecognitionResultStatus.Success)
                {
                    Debug.WriteLine("SpeechManager: CompileConstraintsAsync failed for phrases");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        async Task CompileDictationConstraint()
        {
            SpeechRecognizer.Constraints.Clear();

            // Apply the dictation topic constraint to optimize for dictated freeform speech.
            SpeechRecognizer.Constraints
                            .Add(new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, 
                                                                      "dictation"));

            var result = await SpeechRecognizer.CompileConstraintsAsync();
            if (result.Status != SpeechRecognitionResultStatus.Success)
            {
                Debug.WriteLine("SpeechRecognizer.CompileConstraintsAsync failed for dictation");
            }
        }

        async Task CompilePhraseConstraints()
        {
            try
            {
                PopulatePhrases();
                await CompilePhrasesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        void OnContinuousRecognitionSessionCompleted(
            SpeechContinuousRecognitionSession sender,
            SpeechContinuousRecognitionCompletedEventArgs args)
        {
            _isInRecognitionSession = false;            
            OnStateChanged(new StateChangedEventArgs(args));
        }

        /// <summary>
        /// Handle events fired when a result is generated. This may include a garbage rule that fires when general room noise
        /// or side-talk is captured (this will have a confidence of Rejected typically, but may occasionally match a rule with
        /// low confidence).
        /// </summary>
        void OnContinuousRecognitionSessionResultGenerated(
            SpeechContinuousRecognitionSession sender,
            SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Status != SpeechRecognitionResultStatus.Success)
            {
                return;
            }

            // Unpack event arg data.
            bool hasConstraint = args.Result.Constraint != null;
            var confidence = args.Result.Confidence;
            string phrase = args.Result.Text;

            // The garbage rule doesn't have a tag associated with it, and 
            // the other rules return a string matching the tag provided
            // when the grammar was compiled.
            string tag = hasConstraint ? args.Result.Constraint.Tag : "unknown";
            if (tag == "unknown")
            {
                return;
            }

            if (hasConstraint && args.Result.Constraint.Type == SpeechRecognitionConstraintType.List)
            {
                // The List constraint type represents speech from 
                // a compiled grammar of commands.
                var command = _intentInterpreter.GetPhraseIntent(phrase);

                // You may decide to use per-phrase confidence levels in order to 
                // tune the behavior of your grammar based on testing.
                if (confidence == SpeechRecognitionConfidence.Medium ||
                    confidence == SpeechRecognitionConfidence.High)
                {
                    OnPhraseRecognized(new PhraseRecognizedEventArgs(phrase,
                                                                     command,
                                                                     args));
                }
            }
            else if (hasConstraint && args.Result.Constraint.Type == SpeechRecognitionConstraintType.Topic)
            {
                // The Topic constraint type represents speech from dictation.                
                OnPhraseRecognized(new PhraseRecognizedEventArgs(phrase,
                                                                 Command.Dictation,
                                                                 args));
            }
        }

        /// <summary>
        /// Provides feedback to client code based on whether the recognizer is receiving speech input.
        /// </summary>
        void OnSpeechRecognizerStateChanged(SpeechRecognizer sender, 
                                            SpeechRecognizerStateChangedEventArgs args)
            => OnStateChanged(new StateChangedEventArgs(args));

        async Task ISpeechEngine.EndRecognitionSessionAsync()
        {
            // Detach event handlers.
            if (SpeechRecognizer != null && SpeechRecognizer.ContinuousRecognitionSession != null)
            {
                SpeechRecognizer.ContinuousRecognitionSession.Completed -= OnContinuousRecognitionSessionCompleted;
                SpeechRecognizer.ContinuousRecognitionSession.ResultGenerated -= OnContinuousRecognitionSessionResultGenerated;
            }

            // Stop the recognition session, if it's in progress.
            if (_isInRecognitionSession)
            {
                try
                {
                    if (SpeechRecognizer.State != SpeechRecognizerState.Idle)
                    {
                        await SpeechRecognizer.ContinuousRecognitionSession.CancelAsync();
                    }
                    else
                    {
                        await SpeechRecognizer.ContinuousRecognitionSession.StopAsync();
                    }

                    _isInRecognitionSession = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        void OnMediaElementMediaEnded(object sender, RoutedEventArgs e)
        {
            // Signal the SpeakAsync method.
            Semaphore.Release();
        }
    }
}