using Api;
using System.Windows.Input;

namespace ViewModels
{
    public class NavbarViewModel : ViewModelBase
    {
        private readonly PersistenceApiClient _persistenceApi;

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
            set => SetProperty(ref _persistenceIcon, value); // 1. Esto avisa a la vista automáticamente
        }

        public ICommand TogglePersistenceCommand { get; }

        // Evento para notificar al padre
        public event EventHandler? PersistenceModeChanged;

        public NavbarViewModel()
        {
            _persistenceApi = new PersistenceApiClient();
            TogglePersistenceCommand = new AsyncRelayCommand(TogglePersistenceAsync);
            _ = LoadPersistenceIconAsync();
        }

        private async Task LoadPersistenceIconAsync()
        {
            try
            {
                var mode = await _persistenceApi.GetPersistenceModeAsync();

                // 2. Al ejecutar esta línea, se dispara el SET de arriba y la imagen cambia
                if (mode.Equals("Database", StringComparison.OrdinalIgnoreCase))
                    PersistenceIcon = "/Assets/Icons/database.png";
                else
                    PersistenceIcon = "/Assets/Icons/memory.png";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar icono: {ex.Message}");
            }
        }

        private async Task TogglePersistenceAsync(object? parameter)
        {
            try
            {
                var current = await _persistenceApi.GetPersistenceModeAsync();
                string next = current.Equals("Database", StringComparison.OrdinalIgnoreCase) ? "memory" : "Database";

                bool ok = await _persistenceApi.SetPersistenceAsync(next);

                if (!ok)
                {
                    System.Windows.MessageBox.Show("No se pudo cambiar la persistencia.");
                    return;
                }

                // 3. Recargamos el icono. Esto llamará al 'set' de la propiedad y actualizará la UI.
                await LoadPersistenceIconAsync();

               

                PersistenceModeChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error cambiando persistencia: {ex.Message}");
            }
        }
    }
}