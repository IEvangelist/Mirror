using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mirror.Commands
{
    interface ICommand
    {
        IEnumerable<string> CommandAliases { get; }

        Task ExecuteAsync();
    }
}