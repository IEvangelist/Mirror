using System.Threading.Tasks;

namespace Mirror.Threading
{
    public class TaskCache<T>
    {
        public static Task<T> Result { get; } = Task.FromResult(default(T));      
    }
}