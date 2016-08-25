using Mirror.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using static Windows.ApplicationModel.Package;


namespace Mirror.IO
{
    public interface IAudioService
    {
        Task<IEnumerable<StorageFile>> GetAudioFilesAsync();
    }

    public class AudioService : IAudioService
    {
        async Task<IEnumerable<StorageFile>> IAudioService.GetAudioFilesAsync()
        {
            var musicFolder = await Current.InstalledLocation.GetFolderAsync(@"Assets\Music");
            var files = await musicFolder.GetAllFilesAsync();

            return files;
        }
    }
}