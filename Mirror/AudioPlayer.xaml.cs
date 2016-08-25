using Mirror.Core;
using Mirror.Extensions;
using Mirror.IO;
using Mirror.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Mirror
{
    public sealed partial class AudioPlayer : UserControl
    {
        public event EventHandler<Song> TrackChanged;

        IEnumerable<StorageFile> _songs;
        IAudioService _audioService = Services.Get<IAudioService>();

        public AudioPlayer()
        {
            InitializeComponent();
        }

        async void OnLoaded(object sender, RoutedEventArgs e) => await LoadAudioAsync();

        async Task LoadAudioAsync()
        {
            //if (true ) /// DesignMode.DesignModeEnabled)
            //{
            //    return;
            //}

            _songs = _songs ?? await _audioService.GetAudioFilesAsync();

            var song = _songs.RandomElement();
            var stream = await song.OpenStreamForReadAsync();

            var file = TagLib.File.Create(new StreamFileAbstraction(song.Name, stream, null));
            
            TrackChanged?.Invoke(this, new Song(file.Tag));
            
            _mediaElement.SetSource(await song.OpenReadAsync(), song.ContentType);
            _mediaElement.Play();
        }
    }
}