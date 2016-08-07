using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Mirror.Commands
{
    class VolumeCommand : ICommand
    {
        double _percent;

        internal VolumeCommand(double percent)
        {
            if (percent < 0 || percent > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(percent));
            }

            _percent = percent;
        }

        IEnumerable<string> ICommand.CommandAliases
            => new[] { "volume" };

        Task ICommand.ExecuteAsync()
        {
            ElementSoundPlayer.Volume = _percent;

            return Task.CompletedTask;
        }
    }
}