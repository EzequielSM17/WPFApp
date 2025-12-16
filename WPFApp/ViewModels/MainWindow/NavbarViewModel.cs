using Api;
using DTOs;
using Events;
using Service.Interfaces;
using System.Text.Json;
using System.Windows.Input;
using VoiceGamesClient.Voice;


namespace ViewModels
{
    public class NavbarViewModel : ViewModelBase, IDisposable
    {
        private readonly PersistenceApiClient _persistenceApi;
        private readonly AgentApiClient _agentApi;
        private readonly SpeechService _speechService;
        private readonly INavigationService _navigationService;
        private readonly IDispatcherService _dispatcherService;

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

        public NavbarViewModel(INavigationService navigationService, IDispatcherService dispatcherService)
        {
            _navigationService = navigationService;
            _dispatcherService = dispatcherService;
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
                    _navigationService.ShowError("Error micrófono: " + ex.Message);
                }
            }
        }

        private void OnListeningStopped()
        {
            _dispatcherService.Invoke(() =>
            {
                _isListening = false;
                MicIcon = "/Assets/Icons/mic_off.png";
                MicBackground = "White";
            });
        }

        private async void OnVoiceTextRecognized(string text)
        {
            _speechService.StopListening();

            IsProcessing = true;
            MicIcon = "/Assets/Icons/loading.png";
            MicBackground = "#FFD700";

            try
            {
                var response = await _agentApi.SendVoiceCommandAsync(text);

                if (response != null)
                {
                    await HandleAgentResponse(response);
                }
                else
                {
                    _navigationService.ShowError("El agente no respondió correctamente.");
                }
            }
            catch (Exception ex)
            {
                _navigationService.ShowError("Error de conexión: " + ex.Message);
            }
            finally
            {
                IsProcessing = false;

                _dispatcherService.Invoke(() =>
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
                            if (response.Result is PagedDTO<GameDTOWithId> list)
                            {
                                GamesReceived?.Invoke(this, new GamesReceivedEventArgs(list));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _navigationService.ShowError("Error procesando datos del agente: " + ex.Message);
                    }
                    break;

                case "add_game":
                case "delete_game":
                case "update_game":
                case "change_persistence":
                    await LoadPersistenceIconAsync();
                    PersistenceModeChanged?.Invoke(this, EventArgs.Empty);
                    _navigationService.ShowMessage("Agente: Operación realizada.", "Éxito");
                    break;

                default:
                    _navigationService.ShowMessage($"Agente ejecutó: {response.Tool}");
                    break;
            }
        }

        private async Task<string> LoadPersistenceIconAsync()
        {
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
                string mode = await _persistenceApi.GetPersistenceModeAsync();
                if (mode.Equals("Database", StringComparison.OrdinalIgnoreCase))
                {
                    await _persistenceApi.SetPersistenceAsync("memory");
                }
                else
                {
                    await _persistenceApi.SetPersistenceAsync("database");
                }
                await LoadPersistenceIconAsync();
                PersistenceModeChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _navigationService.ShowError("Error al cambiar persistencia: " + ex.Message);
            }
        }

        public void Dispose()
        {
            _speechService?.Dispose();
        }
    }
}