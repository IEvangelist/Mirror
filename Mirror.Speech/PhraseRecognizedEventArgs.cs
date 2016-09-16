using System;
using Windows.Media.SpeechRecognition;

namespace Mirror.Speech
{
    public class PhraseRecognizedEventArgs : EventArgs
    {
        /// <summary>
        /// The phrase provided by the speech recognizer.
        /// </summary>
        public string PhraseText { get; private set; }

        /// <summary>
        /// Gets the intent of the phrase.
        /// </summary>
        public CommandContext CommandContext { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the speech recognizer is listening
        /// for dictated speech instead of a command list.
        /// </summary>
        public bool IsDictation { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhraseRecognizedEventArgs"/> class.
        /// </summary>
        /// <param name="person">The Person who the note is addressed to.</param>
        /// <param name="phrase">The phrase provided by the speech recognizer.</param>
        /// <param name="speechRecognitionArgs">Event data from the speech recognizer.</param>
        public PhraseRecognizedEventArgs(
            string phrase,
            CommandContext commandContext,
            SpeechContinuousRecognitionResultGeneratedEventArgs speechRecognitionArgs)
        {
            PhraseText = phrase;
            CommandContext = commandContext;
            IsDictation = 
                speechRecognitionArgs.Result.Constraint == null 
                ? false 
                : CommandContext.Command == Command.Dictation;
        }
    }
}