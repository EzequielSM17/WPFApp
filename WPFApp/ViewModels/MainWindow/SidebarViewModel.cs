using System.Windows.Input;


namespace ViewModels
{
    public class SidebarViewModel : ViewModelBase
    {
        public ICommand CreateGameCommand { get; }
        public ICommand LogoutCommand { get; }

        public SidebarViewModel()
        {
            // La lógica de negocio real se manejará con eventos o servicios
            CreateGameCommand = new RelayCommand(CreateGame);
            LogoutCommand = new RelayCommand(Logout);
        }

        private void CreateGame(object? parameter)
        {
            // NOTIFICAR AL PADRE (MainWindowViewModel) que abra el diálogo de creación
            RequestNewGameDialog?.Invoke(this, EventArgs.Empty);
        }

        private void Logout(object? parameter)
        {
            // La lógica de limpieza de sesión se puede manejar aquí o en el padre
            // La navegación de ventana se maneja en el Code-behind de MainWindow

            // NOTIFICAR AL PADRE (MainWindowViewModel) que cierre la sesión
            RequestLogout?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? RequestNewGameDialog;
        public event EventHandler? RequestLogout;
    }
}