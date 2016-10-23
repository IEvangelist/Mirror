using System.Threading.Tasks;

namespace Mirror.Controls
{
    interface IAsyncLoader
    {
        Task LoadAsync();
    }
}