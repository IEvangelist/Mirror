using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Mirror.Speech
{
    public interface ISpeechEngine
    {
        SpeechRecognitionMode RecognitionMode { get; }

        event EventHandler<PhraseRecognizedEventArgs> PhraseRecognized;

        event EventHandler<StateChangedEventArgs> StateChanged;

        Task SetRecognitionModeAsync(SpeechRecognitionMode mode);

        Task SpeakAsync(string phrase, MediaElement media);

        Task StartContinuousRecognitionAsync();

        Task EndRecognitionSessionAsync();
    }
}