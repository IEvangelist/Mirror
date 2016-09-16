using System;

namespace Mirror.Speech
{
    public struct CommandContext
    {
        public Command Command { get; private set; }

        public DateTime? DateContext { get; private set; }

        public CommandContext(Command command, DateTime? dateContext = null)
        {
            Command = command;
            DateContext = dateContext;
        }

        public static implicit operator Command(CommandContext context) => context.Command;

        public static implicit operator CommandContext(Command command) => new CommandContext(command);
    }
}