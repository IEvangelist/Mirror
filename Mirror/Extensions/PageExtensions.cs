using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using static Windows.UI.Core.CoreDispatcherPriority;


namespace Mirror.Extensions
{
    static class PageExtensions
    {
        public static async Task ThreadSafeAsync(this Page page, Action action)
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
    }
}