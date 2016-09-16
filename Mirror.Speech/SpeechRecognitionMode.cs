namespace Mirror.Speech
{
    public enum SpeechRecognitionMode
    {
        /// <summary>
        /// The default recognition mode, which is a policy defined elsewhere, 
        /// currently in the SpeechManager class.
        /// </summary>
        Default = 0,

        /// <summary>
        /// The speech recognizer listens for a List of commands.
        /// </summary>
        CommandPhrases,

        /// <summary>
        /// The speech recognizer listens for dictation.
        /// </summary>
        Dictation,

        /// <summary>
        /// The speech recognizer isn't listening for speech.
        /// </summary>
        Paused
    }
}