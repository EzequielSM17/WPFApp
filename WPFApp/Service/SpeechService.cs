using System.Globalization;
using System.Speech.Recognition;

namespace VoiceGamesClient.Voice
{
    public class SpeechService : IDisposable
    {
        private readonly SpeechRecognitionEngine _engine;
        private bool _isListening;

        public event Action<string>? TextRecognized;

        public SpeechService()
        {
            // Cultura española
            var culture = new CultureInfo("es-ES");
            _engine = new SpeechRecognitionEngine(culture);

            _engine.SetInputToDefaultAudioDevice();

            // Gramática de dictado libre
            _engine.LoadGrammar(new DictationGrammar());

            _engine.SpeechRecognized += (s, e) =>
            {
                if (e.Result != null && !string.IsNullOrWhiteSpace(e.Result.Text))
                {
                    TextRecognized?.Invoke(e.Result.Text);
                }
            };
        }

        public void StartListening()
        {
            if (_isListening) return;
            _isListening = true;
            _engine.RecognizeAsync(RecognizeMode.Multiple);
        }

        public void StopListening()
        {
            if (!_isListening) return;
            _isListening = false;
            _engine.RecognizeAsyncStop();
        }

        public void Dispose()
        {
            try
            {
                _engine.RecognizeAsyncStop();
            }
            catch { }
            _engine.Dispose();
        }
    }
}
