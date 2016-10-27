using System;
using System.Threading.Tasks;

namespace Mirror.Threading
{
    public class TaskCache<T>
    {
        static Task<T> _value;

        public static Task<T> Default { get; } = Task.FromResult(default(T));        

        public static Task<T> Value(Func<T> getValue) => _value ?? (_value = Task.FromResult(getValue()));
    }
}