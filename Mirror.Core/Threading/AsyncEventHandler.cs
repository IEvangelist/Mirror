using System.Threading.Tasks;

namespace Mirror.Threading
{
    public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs e);
}