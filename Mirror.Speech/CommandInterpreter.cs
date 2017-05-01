using System;
using System.Linq;
using Mirror.Extensions;

namespace Mirror.Speech
{
    public class CommandInterpreter : ICommandInterpreter
    {
        static string[] Days { get; } = Enum.GetNames(typeof(DayOfWeek));

        CommandContext ICommandInterpreter.GetPhraseIntent(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase))
            {
                return Command.None;
            }

            if (phrase.StartsWith("Help", StringComparison.OrdinalIgnoreCase) || Contains(phrase, "What can I say"))
            {
                return Command.Help;
            }
            else if (StartsWith(phrase, "Look") || 
                     Contains(phrase, "How do I look") ||
                     Contains(phrase, "Tell me how I look"))
            {
                return Command.Emotion;
            }
            else if (StartsWithAny(phrase, "What", "Read", "How", "On"))
            {
                if (ContainsAny(phrase, "event", "calendar"))
                {
                    if (Contains(phrase, "on") && TryParseContext(phrase, out var dateContext))
                    {
                        return new CommandContext(Command.CalendarEvents, dateContext);
                    }
                    return Command.CalendarEvents;
                }
                else if (Contains(phrase, "forecast"))
                {
                    return Command.ForecastWeather;
                }
                else if (ContainsAny(phrase, "temp", "weather"))
                {
                    if (Contains(phrase, "on") && TryParseContext(phrase, out var dateContext))
                    {
                        return new CommandContext(Command.ForecastWeather, dateContext);
                    }
                    return Command.CurrentWeather;
                }
            }
            else if (ContainsAny(phrase, "turn", "mute", "volume", "loud", "quiet"))
            {
                return Command.Volume;
            }
            else if (Contains(phrase, "play"))
            {
                return Command.Audio;
            }

            return Command.Dictation;
        }

        static bool StartsWith(string phrase, string match) => phrase.StartsWith(match, StringComparison.OrdinalIgnoreCase);

        static bool StartsWithAny(string phrase, params string[] matches) => matches?.Any(match => StartsWith(phrase, match)) ?? false;

        static bool Contains(string phrase, string match) => phrase.ContainsIgnoringCase(match);

        static bool ContainsAny(string phrase, params string[] matches) => matches?.Any(match => Contains(phrase, match)) ?? false;

        static bool TryParseContext(string phrase, out DateTime? dateContext)
        {
            dateContext = null;
            foreach (var day in Days)
            {
                if (Contains(phrase, day))
                {
                    dateContext = DateTime.Now.Next(day.ToEnum<DayOfWeek>());
                    return true;
                }
            }

            if (Contains(phrase, "today"))
            {
                dateContext = DateTime.Now;
                return true;
            }
            else if (Contains(phrase, "tomorrow"))
            {
                dateContext = DateTime.Now.AddDays(1);
                return true;
            }

            return false;
        }
    }
}