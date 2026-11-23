using System.IO;
using System.Windows;
using Mosaic.UI.Wpf.Controls.Scripting;
using Mosaic.UI.Wpf.Logging;
using NAudio.Wave;

namespace Avalon.Scripting;

/// <summary>
/// The <c>audio</c> object: plays sound files and the bundled notification sounds.
/// </summary>
[ScriptModule(Name = "audio", Description = "Plays a sound file or one of the five notification sounds.")]
public sealed class AudioScriptCommands
{
    // All players are created, reused, and disposed on the application dispatcher.
    private static readonly Dictionary<string, AudioPlayer> NotificationPlayers = new();
    private static AudioPlayer? _generalPlayer;

    /// <summary>
    /// Plays an audio file (wav, mp3, m4a, m4r, wma).
    /// </summary>
    public void Play(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The audio file was not found.", filePath);
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            _generalPlayer?.Dispose();
            _generalPlayer = null;
            _generalPlayer = new AudioPlayer(filePath);
            _generalPlayer.PlayFromStart();
        });
    }

    /// <summary>
    /// Plays the first notification sound.
    /// </summary>
    public void PlayNotify1() => PlayBundled("notify1.m4r");

    /// <summary>
    /// Plays the second notification sound.
    /// </summary>
    public void PlayNotify2() => PlayBundled("notify2.m4r");

    /// <summary>
    /// Plays the third notification sound.
    /// </summary>
    public void PlayNotify3() => PlayBundled("notify3.m4r");

    /// <summary>
    /// Plays the fourth notification sound.
    /// </summary>
    public void PlayNotify4() => PlayBundled("notify4.m4r");

    /// <summary>
    /// Plays the fifth notification sound.
    /// </summary>
    public void PlayNotify5() => PlayBundled("notify5.m4r");

    private static void PlayBundled(string name)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (!NotificationPlayers.TryGetValue(name, out var player))
            {
                var path = Path.Combine(AppContext.BaseDirectory, "Assets", "Audio", name);
                player = new AudioPlayer(path);
                NotificationPlayers.Add(name, player);
            }

            player.PlayFromStart();
        });
    }

    /// <summary>
    /// Releases the shared audio players when the application exits.
    /// </summary>
    internal static void ReleasePlayers()
    {
        _generalPlayer?.Dispose();
        _generalPlayer = null;
        foreach (var player in NotificationPlayers.Values)
        {
            player.Dispose();
        }

        NotificationPlayers.Clear();
    }

    private sealed class AudioPlayer : IDisposable
    {
        private readonly WaveOut _output;
        private readonly AudioFileReader _reader;

        public AudioPlayer(string path)
        {
            _reader = new AudioFileReader(path);
            _output = new WaveOut();
            try
            {
                _output.Init(_reader);
            }
            catch
            {
                Dispose();
                throw;
            }

            _output.PlaybackStopped += (_, args) =>
            {
                if (args.Exception is { } error)
                {
                    Logger.LogError($"Could not play audio '{path}'. {error.Message}");
                }
            };
        }

        public void PlayFromStart()
        {
            _output.Stop();
            _reader.Position = 0;
            _output.Play();
        }

        public void Dispose()
        {
            _output.Dispose();
            _reader.Dispose();
        }
    }
}
