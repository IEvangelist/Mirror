using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using static Windows.UI.Core.CoreDispatcherPriority;


namespace Mirror.Extensions
{
    public static class DependencyObjectExtensions
    {
        public static async Task ThreadSafeAsync(this DependencyObject page, Action action)
        {
            if (null == page) return;

            if (page.Dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                await page.Dispatcher.RunAsync(Normal, () => action());
            }
        }

        public static async Task<T> ThreadSafeAsync<T>(this DependencyObject page, Func<T> func)
        {
            if (null == page) return default(T);

            if (page.Dispatcher.HasThreadAccess)
            {
                return func();
            }
            else
            {
                T result = default(T);
                await page.Dispatcher.RunAsync(Normal, () => result = func());
                return result;
            }
        }
    }
}