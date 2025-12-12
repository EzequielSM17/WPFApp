using Api;
using DTOs;
using Events;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows; // Para MessageBox y Dispatcher
using System.Windows.Input;
using VoiceGamesClient.Voice; // Tu namespace del SpeechService

namespace ViewModels
{
    public class NavbarViewModel : ViewModelBase, IDisposable
    {
        private readonly PersistenceApiClient _persistenceApi;
        private readonly AgentApiClient _agentApi;
        private readonly SpeechService _speechService;

        // --- Propiedades ---

        private string _userEmail = string.Empty;
        public string UserEmail
        {
            get => _userEmail;
            set => SetProperty(ref _userEmail, value);
        }

        private string _persistenceIcon = "/Assets/Icons/memory.png";
        public string PersistenceIcon
        {
            get => _persistenceIcon;
            set => SetProperty(ref _persistenceIcon, value);
        }

        // Icono del micrófono (cambia si escucha o no)
        private string _micIcon = "/Assets/Icons/mic_off.png";
        public string MicIcon
        {
            get => _micIcon;
            set => SetProperty(ref _micIcon, value);
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }
        private string _micBackground = "White";
        public string MicBackground
        {
            get => _micBackground;
            set => SetProperty(ref _micBackground, value);
        }

        private bool _isListening;


        public ICommand TogglePersistenceCommand { get; }
        public ICommand ToggleVoiceCommand { get; }

        
        public event EventHandler? PersistenceModeChanged;
        public event EventHandler<GamesReceivedEventArgs>? GamesReceived;
        public NavbarViewModel()
        {
            _persistenceApi = new PersistenceApiClient();
            _agentApi = new AgentApiClient();
            _speechService = new SpeechService();
            _speechService.TextRecognized += OnVoiceTextRecognized;

            _speechService.ListeningStopped += OnListeningStopped;
            
            TogglePersistenceCommand = new AsyncRelayCommand(TogglePersistenceAsync);
            ToggleVoiceCommand = new RelayCommand(ToggleVoice);

            _ = LoadPersistenceIconAsync();
        }


        private void ToggleVoice(object? parameter)
        {
            if (_speechService == null) return;

            if (_isListening)
            {
                _speechService.StopListening();
            }
            else
            {
                try
                {
                    _speechService.StartListening();
                    _isListening = true;
                    MicIcon = "/Assets/Icons/mic_on.png";
                    MicBackground = "#FFCCCC";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error micrófono: " + ex.Message);
                }
            }
        }
        private void OnListeningStopped()
        {
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                _isListening = false;
                MicIcon = "/Assets/Icons/mic_off.png";
                MicBackground = "White";
                
            });
        }

        private async void OnVoiceTextRecognized(string text)
        {
            // A. Detenemos escucha
            _speechService.StopListening();

            // B. ACTIVAMOS MODO CARGANDO
            IsProcessing = true;
            MicIcon = "/Assets/Icons/loading.png"; // Pon aquí tu icono de carga
            MicBackground = "#FFD700"; // Opcional: Color Amarillo/Naranja mientras piensa

            try
            {
                // C. Llamada al Agente (Aquí es donde tarda)
                var response = await _agentApi.SendVoiceCommandAsync(text);

                if (response != null)
                {
                    await HandleAgentResponse(response);
                }
                else
                {
                    MessageBox.Show("El agente no respondió correctamente.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión: " + ex.Message);
            }
            finally
            {
                // D. DESACTIVAMOS MODO CARGANDO (Siempre, aunque falle)
                IsProcessing = false;

                // Restauramos estado original
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _isListening = false;
                    MicIcon = "/Assets/Icons/mic_off.png";
                    MicBackground = "White";
                });
            }
        }

        private async Task HandleAgentResponse(AgentResponse response)
        {
            switch (response.Tool)
            {
                case "list_games":
                    try
                    {

                        if (response.Result is JsonElement element)
                        {
                            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            var gamesList = JsonSerializer.Deserialize<PagedDTO<GameDTOWithId>>(element.GetRawText(), options);

                            if (gamesList?.Items != null)
                            {
                                GamesReceived?.Invoke(this, new GamesReceivedEventArgs(gamesList));
                            }
                        }
                        else
                        {
                            if (response.Result is  PagedDTO<GameDTOWithId> list)
                            {
                                GamesReceived?.Invoke(this, new GamesReceivedEventArgs(list));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error procesando datos del agente: " + ex.Message); 
                    }
                    break;

                case "add_game":
                case "delete_game":
                case "update_game":
                case "change_persistence":
                    await LoadPersistenceIconAsync();
                    PersistenceModeChanged?.Invoke(this, EventArgs.Empty);
                    MessageBox.Show("Agente: Operación realizada.");
                    break;

                default:
                    MessageBox.Show($"Agente ejecutó: {response.Tool}");
                    break;
            }
        }


        private async Task<string> LoadPersistenceIconAsync()
        {
            // ... (Tu código existente) ...
            try
            {
                var mode = await _persistenceApi.GetPersistenceModeAsync();
                if (mode.Equals("Database", StringComparison.OrdinalIgnoreCase))
                    PersistenceIcon = "/Assets/Icons/database.png";
                else
                    PersistenceIcon = "/Assets/Icons/memory.png";

                return mode;
            }
            catch { return ""; }
        }

        private async Task TogglePersistenceAsync(object? parameter)
        {
            try
            {
                
                string mode=await LoadPersistenceIconAsync();
                if(mode.Equals("Database", StringComparison.OrdinalIgnoreCase))
                {
                    await _persistenceApi.SetPersistenceAsync("memory");
                }
                else
                {
                    await _persistenceApi.SetPersistenceAsync("database");
                }

                    PersistenceModeChanged?.Invoke(this, EventArgs.Empty);
            }
            catch { }
        }

        public void Dispose()
        {
            _speechService?.Dispose();
        }
    }
}