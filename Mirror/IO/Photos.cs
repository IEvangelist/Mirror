using Mirror.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;


namespace Mirror.IO
{
    static class Photos
    {
        const string PhotoName = "temporary";
        const string PhotoNameWithExtension = PhotoName + ".jpg";

        internal static IAsyncOperation<StorageFile> CreateAsync() =>
            KnownFolders.PicturesLibrary
                        .CreateFileAsync(PhotoNameWithExtension,
                                         CreationCollisionOption.GenerateUniqueName);

        internal static async Task CleanupAsync()
        {
            var files = await KnownFolders.PicturesLibrary.GetFilesAsync();

            var deletions =
                files.Where(file => file.DisplayName.Contains(PhotoName))
                     .Select(file => file.DeleteAsync(StorageDeleteOption.PermanentDelete))
                     .AsTasks();
            
            await Task.WhenAll(deletions);
        }
    }
}