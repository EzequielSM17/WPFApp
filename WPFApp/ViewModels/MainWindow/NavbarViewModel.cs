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

        // Color de fondo del botón para feedback visual
        private string _micBackground = "White";
        public string MicBackground
        {
            get => _micBackground;
            set => SetProperty(ref _micBackground, value);
        }

        private bool _isListening;

        // --- Comandos y Eventos ---

        public ICommand TogglePersistenceCommand { get; }
        public ICommand ToggleVoiceCommand { get; }

        
        public event EventHandler? PersistenceModeChanged;
        public event EventHandler<GamesReceivedEventArgs>? GamesReceived;
        public NavbarViewModel()
        {
            _persistenceApi = new PersistenceApiClient();
            _agentApi = new AgentApiClient();
            _speechService = new SpeechService();

            // Inicializamos el servicio de voz
            try
            {
                
                _speechService.TextRecognized += OnVoiceTextRecognized;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo iniciar el servicio de voz: " + ex.Message);
            }

            TogglePersistenceCommand = new AsyncRelayCommand(TogglePersistenceAsync);
            ToggleVoiceCommand = new RelayCommand(ToggleVoice);

            _ = LoadPersistenceIconAsync();
        }

        // --- Lógica de Voz ---

        private void ToggleVoice(object? parameter)
        {
            if (_speechService == null) return;

            if (_isListening)
            {
                _speechService.StopListening();
                _isListening = false;
                MicIcon = "/Assets/Icons/mic_off.png";
                MicBackground = "White";
            }
            else
            {
                try
                {
                    _speechService.StartListening();
                    _isListening = true;
                    MicIcon = "/Assets/Icons/mic_on.png"; // Asegúrate de tener este icono
                    MicBackground = "#FFCCCC"; // Rojo claro para indicar grabación
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al activar micrófono: " + ex.Message);
                }
            }
        }

        private async void OnVoiceTextRecognized(string text)
        {
            // Detenemos la escucha para procesar (UX: "Walkie-talkie" style)
            // Opcional: puedes dejarlo escuchando si prefieres comandos continuos
            Application.Current.Dispatcher.Invoke(() => ToggleVoice(null));

            

            // Llamada al Agente MCP
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

        private async Task HandleAgentResponse(AgentResponse response)
        {
            switch (response.Tool)
            {
                case "list_games":
                    try
                    {
                        // 2. MAGIA DE DESERIALIZACIÓN
                        // El Result viene como un JsonElement (objeto genérico).
                        // Tenemos que convertirlo a List<GameDTOWithId>.

                        if (response.Result is JsonElement element)
                        {
                            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                            // A veces el resultado es directo una lista, a veces un objeto paginado.
                            // Asumiremos que el Agente devuelve una lista de items o intentamos mapearlo.

                            // Opción A: El agente devolvió directamente la lista [ {..}, {..} ]
                            var gamesList = JsonSerializer.Deserialize<PagedDTO<GameDTOWithId>>(element.GetRawText(), options);

                            if (gamesList?.Items != null)
                            {
                                // 3. LANZAR EL EVENTO CON LOS DATOS
                                GamesReceived?.Invoke(this, new GamesReceivedEventArgs(gamesList));
                                
                            }
                        }
                        else
                        {
                            // Si por alguna razón ya vino deserializado (raro en este flujo)
                            if (response.Result is  PagedDTO<GameDTOWithId> list)
                            {
                                GamesReceived?.Invoke(this, new GamesReceivedEventArgs(list));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error procesando datos del agente: " + ex.Message);
                        // Fallback: Si falla la conversión, pedimos recarga normal
                        
                    }
                    break;

                case "add_game":
                case "delete_game":
                case "update_game":
                case "change_persistence":
                    // Aquí NO tenemos la lista nueva, así que pedimos recarga
                    await LoadPersistenceIconAsync();
                    PersistenceModeChanged?.Invoke(this, EventArgs.Empty);
                    MessageBox.Show("Agente: Operación realizada.");
                    break;

                default:
                    MessageBox.Show($"Agente ejecutó: {response.Tool}");
                    break;
            }
        }

        // --- Lógica de Persistencia (Existente) ---
        private async Task LoadPersistenceIconAsync()
        {
            // ... (Tu código existente) ...
            try
            {
                var mode = await _persistenceApi.GetPersistenceModeAsync();
                if (mode.Equals("Database", StringComparison.OrdinalIgnoreCase))
                    PersistenceIcon = "/Assets/Icons/database.png";
                else
                    PersistenceIcon = "/Assets/Icons/memory.png";
            }
            catch { }
        }

        private async Task TogglePersistenceAsync(object? parameter)
        {
            try
            {
                await LoadPersistenceIconAsync();
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