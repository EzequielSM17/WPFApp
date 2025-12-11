using DTOs;             
using System.Windows;
using ViewModels; 

namespace WPFApp
{
    public partial class MainWindow : Window
    {
        // Conveniencia: acceder al DataContext fuertemente tipado
        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

        public MainWindow()
        {
            // 1. Instanciar el ViewModel
            var viewModel = new MainWindowViewModel();
            DataContext = viewModel;

            InitializeComponent();

            // 2. Suscribirse a las peticiones de navegación/diálogo de los ViewModels hijos

            // Peticiones de la Sidebar
            ViewModel.SidebarVM.RequestNewGameDialog += Sidebar_RequestNewGameDialog;
            ViewModel.SidebarVM.RequestLogout += Sidebar_RequestLogout;

            // Petición de GameCard
            ViewModel.GameSelectedCommand = new RelayCommand(HandleGameSelected);
        }

        // --- Manejo de la lógica de ventanas y navegación (Responsabilidad del Code-Behind) ---

        /// <summary>
        /// Maneja la solicitud de crear un nuevo juego (emitida por SidebarViewModel).
        /// </summary>
        private async void Sidebar_RequestNewGameDialog(object? sender, EventArgs e)
        {
            // Nota: Aquí se asume que GameEditWindow existe y puede recibir un DTO para crear.

            // 1. Preparar el DTO por defecto (La mejor práctica sería que el VM padre lo hiciera)
            var newGame = new GameDTOWithId
            {
                Id = 0,
                Title = "Nuevo juego",
                Publisher  = "",
                Developer  = "",
                Category  ="",
                Amount =0,
                IsActive =true,
                UrlImagen  = "",
                ReleaseDate =new DateTime()
    };

            var editWindow = new GameEditWindow(newGame, isNew: true);
            editWindow.Owner = this;
            var result = editWindow.ShowDialog();

            // 2. Si el diálogo fue exitoso, notificar al VM para recargar.
            if (result == true)
            {
                // La recarga se debe disparar en el VM
                await ViewModel.LoadGamesAsync(ViewModel.PaginationVM.CurrentPage);
            }
        }

        /// <summary>
        /// Maneja la solicitud de cerrar sesión (emitida por SidebarViewModel).
        /// </summary>
        private void Sidebar_RequestLogout(object? sender, EventArgs e)
        {
            // 1. La limpieza de Session ya la hace el ViewModel.

            // 2. Manejo de la navegación (Cerrar esta y abrir LoginWindow).
            var login = new LoginWindow(); // Asumiendo que esta clase existe
            login.Show();

            this.Close();
        }

        /// <summary>
        /// Maneja la selección de una tarjeta de juego (desde GameSelectedCommand en MainWindowViewModel).
        /// </summary>
        private async void HandleGameSelected(object? parameter)
        {
            if (parameter is GameDTOWithId game)
            {
                var editWindow = new GameEditWindow(game, isNew: false);
                editWindow.Owner = this;

                // Opcional: refrescar lista tras edición
                await ViewModel.LoadGamesAsync(ViewModel.PaginationVM.CurrentPage);
            }
        }

        // El antiguo método MouseLeftButtonUp del XAML debe eliminarse o manejarse con Command, 
        // tal como se hizo con el nuevo 'GameSelectedCommand'.

        /* private void GameCard_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // ESTE MÉTODO DEBE ELIMINARSE. Su lógica se ha movido a HandleGameSelected.
        }
        */
    }
}