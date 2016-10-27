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
        const string AssetMusicPath = @"Assets\Music";

        async Task<IEnumerable<StorageFile>> IAudioService.GetAudioFilesAsync()
        {
            var musicFolder = await Current.InstalledLocation.GetFolderAsync(AssetMusicPath);
            return await musicFolder.GetAllFilesAsync();
        }
    }
}