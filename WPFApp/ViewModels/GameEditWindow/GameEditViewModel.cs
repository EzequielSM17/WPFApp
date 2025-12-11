using Api;
using DTOs;
using System.Windows.Input;
using ViewModels;

namespace WPFApp.ViewModels
{
    public class GameEditViewModel : ViewModelBase
    {
        private readonly GamesApiClient _api;
        private readonly bool _isNew;
        private readonly Action<bool?> _closeWindowAction; // Delegado para cerrar la ventana

        // Objeto que se está editando en la pantalla
        public GameDTOWithId EditableGame { get; private set; }

        // Título de la ventana (Dinámico)
        public string WindowTitle => _isNew ? "Crear nuevo juego" : $"Editar juego #{EditableGame.Id}";

        // Mensaje de error (Feedback visual)
        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // Constructor
        public GameEditViewModel(GameDTOWithId game, bool isNew, Action<bool?> closeWindowAction)
        {
            _api = new GamesApiClient();
            _isNew = isNew;
            _closeWindowAction = closeWindowAction;

            // IMPORTANTE: CLONAR EL OBJETO
            // No editamos 'game' directamente para poder cancelar los cambios.
            // Creamos una copia nueva en EditableGame.
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
            CancelCommand = new RelayCommand(_ => _closeWindowAction(false)); // Cierra con false
        }

        private async Task SaveAsync(object? parameter)
        {
            ErrorMessage = string.Empty;

            try
            {
                bool success = false;

                if (_isNew)
                {
                    // Lógica de Crear
                    var created = await _api.CreateGameAsync(EditableGame);
                    success = created != null;
                }
                else
                {
                    // Lógica de Actualizar
                    success = await _api.UpdateGameAsync(EditableGame.Id, EditableGame);
                }

                if (success)
                {
                    // Si todo sale bien, ejecutamos la acción de cerrar enviando 'true'
                    _closeWindowAction(true);
                }
                else
                {
                    ErrorMessage = _isNew
                        ? "No se pudo crear el juego."
                        : "No se pudo actualizar el juego.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al guardar: {ex.Message}";
            }
        }
    }
}