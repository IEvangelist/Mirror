using System.Threading.Tasks;


namespace Mirror.Commands
{
    class CommandDispatcher
    {
        Task Dispatch(ICommand command) => command?.ExecuteAsync();
    }
}