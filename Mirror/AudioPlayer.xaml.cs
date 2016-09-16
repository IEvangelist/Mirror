using Mirror.Core;
using Mirror.Extensions;
using Mirror.Interfaces;
using Mirror.IO;
using Mirror.Models;
using Mirror.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        Task IVolumeCommandListener.SetVolumeAsync(string phrase) =>
            this.ThreadSafeAsync(() => 
            {
                var volume = _mediaElement.Volume;
                if (phrase.ContainsIgnoringCase("unmute"))
                {
                    _mediaElement.IsMuted = false;
                }
                else if (phrase.ContainsIgnoringCase("mute"))
                {
                    _mediaElement.IsMuted = true;
                }
                else if (phrase.ContainsIgnoringCase("up") ||
                         phrase.ContainsIgnoringCase("loud"))
                {
                    _mediaElement.Volume = Math.Min(1, volume + .1);
                }
                else if (phrase.ContainsIgnoringCase("down") ||
                         phrase.ContainsIgnoringCase("quiet"))
                {
                    _mediaElement.Volume = Math.Max(0, volume - .1);
                }
                else if (phrase.ContainsIgnoringCase("percent"))
                {
                    _mediaElement.Volume = GetPercent(phrase);
                }
            });

        static double GetPercent(string phrase)
        {
            int percent;
            int.TryParse(Regex.Match(phrase, @"\d+").Value, out percent);

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
    }
}