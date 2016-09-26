using Mirror.Extensions;


namespace Mirror.Emotion
{
    public class Result
    {
        public static Result Empty { get; } = new Result();

        public Emotions Emotion { get; private set; }
        public float Score { get; private set; }

        Result() { }

        Result(string emotion, float score) : this()
        {
            Emotion = emotion.To<Emotions>();
            Score = score;
        }

        public static Result FromScore(string emotion, float score) => new Result(emotion, score);
    }
}