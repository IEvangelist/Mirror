using System.Threading.Tasks;

namespace Mirror.Interfaces
{
    public interface IAudioCommandListener
    {
        Task PlayRandomSongAsync();
    }
}