using System;
using System.Threading.Tasks;
using Mirror.Extensions;
using Mirror.ViewModels;
using Windows.UI.Xaml;

namespace Mirror.Controls
{
    public static class SpeechControlHelper
    {
        public static Task<string> GetContextualMessageAsync(
            DependencyObject dependency, 
            object dataContext, 
            DateTime? dateContext = null,
            string unableToGenerateSpeechMessage = "Sorry, I'm unable to process your request.")
            => dependency.ThreadSafeAsync(
                () => dataContext is BaseViewModel viewModel 
                    ? viewModel.ToFormattedString(dateContext) 
                    : unableToGenerateSpeechMessage);
    }
}