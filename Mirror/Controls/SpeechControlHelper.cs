using System;
using System.Threading.Tasks;
using Mirror.ViewModels;
using Mirror.Extensions;
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
            => dependency.ThreadSafeAsync(() => 
            {
                var viewModel = dataContext as BaseViewModel;
                if (viewModel != null)
                {
                    return viewModel.ToFormattedString(dateContext);
                }
                return unableToGenerateSpeechMessage;
            });
    }
}