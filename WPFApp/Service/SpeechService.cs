using System.IO;
using Whisper.net;
using Whisper.net.Ggml;
using NAudio.Wave;

namespace VoiceGamesClient.Voice
{
    public class SpeechService : IDisposable
    {
        private readonly string _modelName = "ggml-base.bin";
        private WhisperFactory? _factory;
        private WhisperProcessor? _processor;
        private WaveInEvent? _waveIn;

        private readonly TimeSpan _silenceTimeout = TimeSpan.FromSeconds(1.5);
        private readonly float _silenceThreshold = 0.02f;
        private DateTime _lastSoundTime;

        private bool _isListening;
        private bool _isStopping; 

        private readonly object _bufferLock = new();
        private readonly MemoryStream _audioBuffer = new();

        public event Action<string>? TextRecognized;
        public event Action? ListeningStopped;

        public SpeechService()
        {
            Task.Run(InitializeModelAsync);
        }

        private async Task InitializeModelAsync()
        {
            if (!File.Exists(_modelName))
            {
                using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.Base);
                using var fileWriter = File.OpenWrite(_modelName);
                await modelStream.CopyToAsync(fileWriter);
            }

            _factory = WhisperFactory.FromPath(_modelName);
            _processor = _factory.CreateBuilder().WithLanguage("es").Build();
        }

        public void StartListening()
        {
          
            if (_isListening || _isStopping || _processor == null) return;

            lock (_bufferLock)
            {
                _audioBuffer.SetLength(0);
            }

            _isListening = true;
            _lastSoundTime = DateTime.Now;

            try
            {
                _waveIn = new WaveInEvent();
                _waveIn.WaveFormat = new WaveFormat(16000, 1);
                _waveIn.BufferMilliseconds = 100;
                _waveIn.DataAvailable += OnAudioDataAvailable;
                _waveIn.StartRecording();
            }
            catch (Exception ex)
            {
                _isListening = false;
                System.Diagnostics.Debug.WriteLine($"Error al iniciar grabación: {ex.Message}");
            }
        }

        private void OnAudioDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (!_isListening) return;

            
            lock (_bufferLock)
            {
                _audioBuffer.Write(e.Buffer, 0, e.BytesRecorded);
            }

           
            if (CheckForSilence(e.Buffer, e.BytesRecorded))
            {
               
                if (DateTime.Now - _lastSoundTime > _silenceTimeout && !_isStopping)
                {
                    
                    Task.Run(() => StopListening());
                }
            }
            else
            {
                _lastSoundTime = DateTime.Now;
            }
        }

        public async void StopListening()
        {
           
            if (_isStopping || !_isListening) return;

            _isStopping = true; 
            _isListening = false; 

            try
            {
               
                if (_waveIn != null)
                {
                    _waveIn.DataAvailable -= OnAudioDataAvailable; 
                    _waveIn.StopRecording();
                    _waveIn.Dispose();
                    _waveIn = null;
                }

                // Avisar a la UI
                ListeningStopped?.Invoke();

                // 2. Procesar el audio (fuera del hilo de grabación)
                float[] samples;
                lock (_bufferLock)
                {
                    _audioBuffer.Position = 0;
                    samples = ConvertPcm16ToFloat(_audioBuffer.ToArray());
                }

                if (samples.Length > 0)
                {
                    await ProcessAudioAsync(samples);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al detener: {ex.Message}");
            }
            finally
            {
                _isStopping = false; // Liberar semáforo
            }
        }

        private async Task ProcessAudioAsync(float[] samples)
        {
            try
            {
                await foreach (var segment in _processor!.ProcessAsync(samples))
                {
                    if (!string.IsNullOrWhiteSpace(segment.Text))
                    {
                        var cleanText = segment.Text.Trim()
                            .Replace("[BLANK_AUDIO]", "")
                            .Replace("[Music]", "");

                        if (cleanText.Length > 0)
                        {
                            TextRecognized?.Invoke(cleanText);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error procesamiento IA: {ex}");
            }
        }

        private bool CheckForSilence(byte[] buffer, int bytesRecorded)
        {
            float maxAmplitude = 0f;

            for (int i = 0; i < bytesRecorded; i += 2)
            {
                short sample = BitConverter.ToInt16(buffer, i);
                float amplitude = Math.Abs(sample / 32768f);

                if (amplitude > maxAmplitude)
                {
                    maxAmplitude = amplitude;
                }
            }

            return maxAmplitude < _silenceThreshold;
        }

        private float[] ConvertPcm16ToFloat(byte[] pcmData)
        {
            var samples = new float[pcmData.Length / 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short sample = BitConverter.ToInt16(pcmData, i * 2);
                samples[i] = sample / 32768f;
            }
            return samples;
        }

        public void Dispose()
        {
            _isListening = false;
            _waveIn?.Dispose();
            _processor?.Dispose();
            _factory?.Dispose();
        }
    }
}