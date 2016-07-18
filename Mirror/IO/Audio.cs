using Mirror.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using static Windows.ApplicationModel.Package;


namespace Mirror.IO
{
    class Audio
    {
        internal static async Task<IEnumerable<StorageFile>> LocalAudioFilesAsync()
        {
            var musicFolder = await Current.InstalledLocation.GetFolderAsync(@"Assets\Music");
            var files = await musicFolder.GetAllFilesAsync();

            return files;
        }
    }
}