using Mirror.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;


namespace Mirror.IO
{
    public interface IPhotoService
    {
        IAsyncOperation<StorageFile> CreateAsync();

        Task CleanupAsync();
    }

    public class PhotoService : IPhotoService
    {
        const string PhotoName = "temporary";
        const string PhotoNameWithExtension = PhotoName + ".jpg";

        IAsyncOperation<StorageFile> IPhotoService.CreateAsync() =>
            KnownFolders.PicturesLibrary
                        .CreateFileAsync(PhotoNameWithExtension,
                                         CreationCollisionOption.GenerateUniqueName);

        async Task IPhotoService.CleanupAsync()
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