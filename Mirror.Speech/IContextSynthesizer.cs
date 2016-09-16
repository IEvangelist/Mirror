using System;
using System.Threading.Tasks;

namespace Mirror.Speech
{
    public interface IContextSynthesizer
    {
        Task<string> GetContextualMessageAsync(DateTime? dateContext = null);
    }
}