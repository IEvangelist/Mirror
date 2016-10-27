using Mirror.Threading;
using System;
using System.Threading.Tasks;
using static System.Threading.Tasks.Task;


namespace Mirror.Core
{
    public static class Do
    {
        public static async Task<T> WithRetry<T>(Func<Task<T>> action, int retryCount = 3)
        {
            int retries = 0;
            while (true)
            {
                try
                {
                    return await action().ConfigureAwait(false);                    
                }
                catch when (++ retries <= retryCount)
                {
                    // Ease up a bit...
                    await Delay(500).ConfigureAwait(false);
                    return await TaskCache<T>.Default;
                }
            }
        }
    }
}