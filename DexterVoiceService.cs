using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using KokoroSharp;
using KokoroSharp.Core;
using KokoroSharp.Utilities;

namespace Dexter
{
    public sealed class DexterVoiceService : IDisposable
    {
        private readonly SemaphoreSlim _speechLock = new(1, 1);

        private KokoroTTS? _tts;
        private KokoroVoice? _voice;
        private bool _isInitialized;
        private bool _isDisposed;

        private string _voiceName = "bm_lewis";

        public async Task InitializeAsync()
        {
            if (_isDisposed || _isInitialized)
                return;

            await Task.Run(() =>
            {
                _tts = KokoroTTS.LoadModel();
                _voice = KokoroVoiceManager.GetVoice(_voiceName);

                foreach (var voice in KokoroVoiceManager.Voices)
                {
                    Debug.WriteLine($"Loaded voice: {voice.Name}");
                }

                _isInitialized = true;
            });
        }

        public async Task SpeakAsync(string text)
        {
            if (_isDisposed || string.IsNullOrWhiteSpace(text))
                return;

            await InitializeAsync();
            await _speechLock.WaitAsync();

            try
            {
                if (_tts == null || _voice == null)
                    return;

                string cleanedText = CleanText(text);

                await Task.Run(() =>
                {
                    _tts.SpeakFast(cleanedText, _voice);
                });
            }
            finally
            {
                _speechLock.Release();
            }
        }

        public void SetVoice(string voiceName)
        {
            if (_isDisposed || string.IsNullOrWhiteSpace(voiceName))
                return;

            _voiceName = voiceName;

            if (_isInitialized)
            {
                _voice = KokoroVoiceManager.GetVoice(_voiceName);
            }
        }

        public void Stop()
        {
            if (_isDisposed)
                return;

            _tts?.StopPlayback();
        }

        private static string CleanText(string text)
        {
            return text
                .Replace("\"", "")
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            try
            {
                _tts?.StopPlayback();
            }
            catch
            {
            }

            _tts?.Dispose();
            _speechLock.Dispose();
        }
    }
}