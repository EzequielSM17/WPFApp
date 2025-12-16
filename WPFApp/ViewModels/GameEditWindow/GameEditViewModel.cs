using Api;
using DTOs;
using System.Threading.Tasks;
using System.Windows; // Necesario para MessageBox
using System.Windows.Input;
using ViewModels;


namespace ViewModels
{
    public class GameEditViewModel : ViewModelBase
    {
        private readonly GamesApiClient _api;
        private readonly bool _isNew;
        private readonly Action<bool?> _closeWindowAction;

        public GameDTOWithId EditableGame { get; private set; }

        public string WindowTitle => _isNew ? "Crear nuevo juego" : $"Editar juego #{EditableGame.Id}";

        // Propiedad para ocultar el botón de eliminar si estamos CREANDO
        public bool IsEditMode => !_isNew;

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; } // <--- NUEVO COMANDO

        public GameEditViewModel(GameDTOWithId game, bool isNew, Action<bool?> closeWindowAction)
        {
            _api = new GamesApiClient();
            _isNew = isNew;
            _closeWindowAction = closeWindowAction;

            EditableGame = new GameDTOWithId
            {
                Id = game.Id,
                Title = game.Title,
                Publisher = game.Publisher,
                Developer = game.Developer,
                Category = game.Category,
                Amount = game.Amount,
                IsActive = game.IsActive,
                UrlImagen = game.UrlImagen,
                ReleaseDate = game.ReleaseDate
            };

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(_ => _closeWindowAction(false));

            // Inicializamos el comando de borrar
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);
        }

        private async Task SaveAsync(object? parameter)
        {
            ErrorMessage = string.Empty;
            try
            {
                bool success = false;
                if (_isNew)
                {
                    var created = await _api.CreateGameAsync(EditableGame);
                    success = created != null;
                }
                else
                {
                    var dto = new GameDTO
                    {
                        Title = EditableGame.Title,
                        Publisher = EditableGame.Publisher,
                        Developer = EditableGame.Developer,
                        Category = EditableGame.Category,
                        Amount = EditableGame.Amount,
                        IsActive = EditableGame.IsActive,
                        UrlImagen = EditableGame.UrlImagen,
                        ReleaseDate = EditableGame.ReleaseDate.ToUniversalTime()
                    };
                    success = await _api.UpdateGameAsync(EditableGame.Id, dto);
                }

                if (success) _closeWindowAction(true);
                else ErrorMessage = _isNew ? "No se pudo crear." : "No se pudo actualizar.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al guardar: {ex.Message}";
            }
        }

        // --- NUEVA LÓGICA DE ELIMINAR ---
        private async Task DeleteAsync(object? parameter)
        {
            // 1. Confirmación de seguridad
            var result = MessageBox.Show(
                $"¿Estás seguro de que quieres eliminar '{EditableGame.Title}'?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            ErrorMessage = string.Empty;

            try
            {
                // 2. Llamada a la API (Asumiendo que existe DeleteGameAsync)
                bool success = await _api.DeleteGameAsync(EditableGame.Id);

                if (success)
                {
                    // Cerramos devolviendo true para que la lista principal se recargue
                    _closeWindowAction(true);
                }
                else
                {
                    ErrorMessage = "No se pudo eliminar el juego.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al eliminar: {ex.Message}";
            }
        }
    }
}