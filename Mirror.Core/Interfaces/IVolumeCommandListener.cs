using System.Threading.Tasks;

namespace Mirror.Interfaces
{
    public interface IVolumeCommandListener
    {
        Task SetVolumeAsync(string phrase);
    }
}