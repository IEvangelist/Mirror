using Mirror.Extensions;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Mirror.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        DependencyObject _dependency;

        public event PropertyChangedEventHandler PropertyChanged;

        public BaseViewModel(DependencyObject dependency)
        {
            _dependency = dependency;
        }

        protected Task OnPropertyChanged(object sender, string propertyName)
        {
            return _dependency.ThreadSafeAsync(
                () => PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName)));
        }

        public abstract string ToFormattedString(DateTime? dateContext);
    }
}