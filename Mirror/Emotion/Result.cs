using Mirror.Extensions;


namespace Mirror.Emotion
{
    class Result
    {
        internal static Result Empty => new Result();

        internal Emotions Emotion { get; private set; }
        internal float Score { get; private set; }

        Result() { }

        Result(string emotion, float score) : this()
        {
            Emotion = emotion.To<Emotions>();
            Score = score;
        }

        internal static Result FromScore(string emotion, float score) => new Result(emotion, score);
    }
}