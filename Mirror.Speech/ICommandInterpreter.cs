namespace Mirror.Speech
{
    public interface ICommandInterpreter
    {
        CommandContext GetPhraseIntent(string phrase);
    }
}