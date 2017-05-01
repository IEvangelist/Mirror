using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mirror.Core;
using Mirror.Extensions;
using Mirror.Interfaces;
using Mirror.IO;
using Mirror.Models;
using Mirror.Threading;
using TagLib;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Mirror
{
    public sealed partial class AudioPlayer : UserControl, IAudioCommandListener, IVolumeCommandListener
    {
        public event AsyncEventHandler<Song> TrackChanged;
        public event AsyncEventHandler<bool> SongEnded; 

        IEnumerable<StorageFile> _songs;
        IAudioService _audioService;

        public AudioPlayer()
        {
            InitializeComponent();
        }

        Task IAudioCommandListener.PlayRandomSongAsync() =>
            this.ThreadSafeAsync(async () =>
            {
                _songs = _songs ?? await _audioService.GetAudioFilesAsync();

                var song = _songs.RandomElement();
                var stream = await song.OpenStreamForReadAsync();

                var file = TagLib.File.Create(new StreamFileAbstraction(song.Name, stream, null));

                await TrackChanged?.Invoke(this, new Song(file.Tag));

                _mediaElement.SetSource(await song.OpenReadAsync(), song.ContentType);
                _mediaElement.Play();
            });

        Task IVolumeCommandListener.SetVolumeAsync(string phrase) => _mediaElement.SetVolumeFromCommandAsync(phrase);           

        static double GetPercent(string phrase)
        {
            int.TryParse(Regex.Match(phrase, @"\d+").Value, out var percent);

            return percent / 100d;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled)
            {
                return;
            }

            _audioService = Services.Get<IAudioService>();
        }

        async void OnMediaEnded(object sender, RoutedEventArgs e) => await SongEnded?.Invoke(this, true);
    }
}