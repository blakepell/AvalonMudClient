/*
 * Lua Automation IDE
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2022 All rights reserved.
 * @license           : Closed Source
 */

using System;
using System.Diagnostics.CodeAnalysis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace MoonSharp.Interpreter.Wpf.Modules
{
    /// <summary>
    /// Audio Script Commands
    /// </summary>
    [MoonSharpModule(Namespace = "audio")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class AudioScriptCommands
    {
        /// <summary>
        /// Shared <see cref="WaveOut"/>.
        /// </summary>
        private WaveOutEvent _waveOut;

        /// <summary>
        /// Shared <see cref="AudioFileReader"/>
        /// </summary>
        private AudioFileReader _audioReader;

        /// <summary>
        /// Shared <see cref="OffsetSampleProvider"/>
        /// </summary>
        private OffsetSampleProvider _offsetSampleProvider;

        /// <summary>
        /// Attempts to play a specified audio file.
        /// </summary>
        /// <param name="fileName"></param>
        [MoonSharpModuleMethod(Description = "Attempts to play a specified audio file.",
                               ParameterCount = 1)]
        public void Play(string fileName)
        {
            this.Stop();

            _waveOut = new WaveOutEvent();
            _audioReader = new AudioFileReader(fileName);

            _waveOut.Init(_audioReader);
            _waveOut.Play();
        }

        /// <summary>
        /// Attempts to play a specified audio file from a starting position in seconds to an ending position in seconds.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="startAt"></param>
        /// <param name="endAt"></param>
        [MoonSharpModuleMethod(Description = "Attempts to play a specified audio file from a starting position in seconds to an ending position in seconds.",
            ParameterCount = 3)]
        public void Play(string fileName, int startAt, int endAt)
        {
            this.Stop();

            _waveOut = new WaveOutEvent();
            _audioReader = new AudioFileReader(fileName);

            _offsetSampleProvider = new OffsetSampleProvider(_audioReader);
            _offsetSampleProvider.SkipOver = TimeSpan.FromSeconds(startAt);
            _offsetSampleProvider.Take = TimeSpan.FromSeconds(endAt);

            _waveOut.Init(_offsetSampleProvider);
            _waveOut.Play();

        }

        /// <summary>
        /// Stops playing any audio that might be playing and frees any associated memory.
        /// </summary>
        [MoonSharpModuleMethod(Description = "Stops playing any audio that might be playing and frees any associated memory.",
            ParameterCount = 0)]
        public void Stop()
        {
            _waveOut?.Stop();
            _audioReader?.Close();
            _audioReader?.Dispose();
            _waveOut?.Dispose();
        }

        /// <summary>
        /// Pauses any playing of audio.
        /// </summary>
        [MoonSharpModuleMethod(Description = "Pauses any playing of audio.",
            ParameterCount = 0)]
        public void Pause() => _waveOut?.Pause();


        /// <summary>
        /// Returns the total time in seconds of the loaded audio file.
        /// </summary>
        /// <returns></returns>
        [MoonSharpModuleMethod(Description = "Returns the total time in seconds of the loaded audio file.",
            ParameterCount = 0)]
        public double TotalTime() => _audioReader?.TotalTime.TotalSeconds ?? 0;

        /// <summary>
        /// Returns the time in seconds a loaded audio file is at.
        /// </summary>
        [MoonSharpModuleMethod(Description = "Returns the time in seconds a loaded audio file is at.",
            ParameterCount = 0)]
        public double AtTime() => _audioReader?.CurrentTime.TotalSeconds ?? 0;

        /// <summary>
        /// If audio is currently playing or not.
        /// </summary>
        [MoonSharpModuleMethod(Description = "If any audio is currently playing.",
            ParameterCount = 0)]
        public bool IsPlaying() => _waveOut?.PlaybackState == PlaybackState.Playing;

        /// <summary>
        /// If any audio is currently paused.
        /// </summary>
        [MoonSharpModuleMethod(Description = "If any audio is currently paused.",
            ParameterCount = 0)]
        public bool IsPaused() => _waveOut?.PlaybackState == PlaybackState.Playing;

    }
}