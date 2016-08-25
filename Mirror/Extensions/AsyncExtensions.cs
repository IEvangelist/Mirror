using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;


namespace Mirror.Extensions
{
    static class AsyncExtensions
    {
        public static IEnumerable<Task> AsTasks(this IEnumerable<IAsyncAction> actions) => actions?.Select(action => action.AsTask());

        public static async Task<IEnumerable<StorageFile>> GetAllFilesAsync(this StorageFolder folder)
        {
            IEnumerable<StorageFile> files = await folder.GetFilesAsync();
            IEnumerable<StorageFolder> folders = await folder.GetFoldersAsync();

            foreach (StorageFolder subfolder in folders)
            {
                files = files.Concat(await subfolder.GetAllFilesAsync());
            }

            return files;
        }
    }
}