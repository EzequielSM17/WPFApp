using Api;
using DTOs;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Utils;


namespace ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly GamesApiClient _gamesApi;
        private readonly int _pageSize = 30;

        // **View Models Hijos**
        public NavbarViewModel NavbarVM { get; }
        public SidebarViewModel SidebarVM { get; }
        public PaginationViewModel PaginationVM { get; }
        // Coordinación de la Navbar

        // Propiedades de la vista principal
        private ObservableCollection<GameDTOWithId> _games = new();
        public ObservableCollection<GameDTOWithId> Games
        {
            get => _games;
            set => SetProperty(ref _games, value);
        }

        public ICommand GameSelectedCommand { get; set; }
        private string? _filterTitle;
        public string? FilterTitle
        {
            get => _filterTitle;
            set => SetProperty(ref _filterTitle, value);
        }

        private string? _filterCategory;
        public string? FilterCategory
        {
            get => _filterCategory;
            set => SetProperty(ref _filterCategory, value);
        }

        private DateTime? _filterFromDate;
        public DateTime? FilterFromDate
        {
            get => _filterFromDate;
            set => SetProperty(ref _filterFromDate, value);
        }

        private DateTime? _filterToDate;
        public DateTime? FilterToDate
        {
            get => _filterToDate;
            set => SetProperty(ref _filterToDate, value);
        }

        
        public ICommand SearchCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        public MainWindowViewModel()
        {
            _gamesApi = new GamesApiClient();

            NavbarVM = new NavbarViewModel();
            SidebarVM = new SidebarViewModel();
            PaginationVM = new PaginationViewModel();

        
            NavbarVM.GamesReceived += (sender, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Games.Clear();
                    if (e.Games.Items != null)
                    {
                        foreach (var game in e.Games.Items)
                        {
                            Games.Add(game);
                        }
                    }
                    PaginationVM.CurrentPage = e.Games.Page;
                    PaginationVM.TotalPages = e.Games.TotalPages;
                    FilterTitle = e.Games.FilterTitle;
                    FilterCategory = e.Games.FilterCategory;
                    FilterFromDate = e.Games.FilterFromDate;
                    FilterToDate = e.Games.FilterToDate;
                });
            };
            // Coordinación de la Navbar
            NavbarVM.PersistenceModeChanged += async (s, e) =>
            {
                // Recarga de juegos al cambiar la persistencia
                await LoadGamesAsync(PaginationVM.CurrentPage);
            };
            NavbarVM.UserEmail = Session.UserEmail; // Asignar propiedad desde el padre

            // Coordinación de la Sidebar (Solicitudes de navegación/diálogo)
            SidebarVM.RequestNewGameDialog += (s, e) => HandleNewGameRequest();
            SidebarVM.RequestLogout += (s, e) => HandleLogoutRequest();

            // Coordinación de la Paginación
            PaginationVM.PageChangeRequested += async (s, page) =>
            {
                await LoadGamesAsync(page);
            };

            // 3. Inicializar la carga de datos
            _ = LoadGamesAsync(1);
            SearchCommand = new AsyncRelayCommand(_ => LoadGamesAsync(1));
            ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);
            // 4. Comandos específicos de MainWindow
            GameSelectedCommand = new RelayCommand(GameSelected);
        }

        // Método central para cargar juegos (utiliza el estado de paginación del PaginationVM)
        private async Task ClearFiltersAsync(object? parameter)
        {
            // Limpiar propiedades (notificará a la vista y borrará los inputs)
            FilterTitle = string.Empty;
            FilterCategory = string.Empty;
            FilterFromDate = null;
            FilterToDate = null;

            // Recargar sin filtros
            await LoadGamesAsync(1);
        }
        public async Task LoadGamesAsync(int page)
        {
            try
            {
                var paged = await _gamesApi.GetGamesAsync(page, _pageSize, FilterTitle,     
                    FilterCategory,  
                    FilterFromDate,  
                    FilterToDate);
                Games.Clear();

                if (paged?.Items != null)
                {
                    foreach (var g in paged.Items)
                        Games.Add(g);

                    // **Actualizar el estado del ViewModel hijo**
                    PaginationVM.TotalPages = paged.TotalPages;
                    PaginationVM.CurrentPage = paged.Page;
                    // Nota: SetProperty en CurrentPage solo se llama si el valor cambia.
                }
                else
                {
                    // **Actualizar el estado del ViewModel hijo**
                    PaginationVM.TotalPages = 1;
                    PaginationVM.CurrentPage = 1;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al cargar juegos: {ex.Message}");
            }
        }

        // Estos métodos solo lanzan eventos o notifican a la vista (a través de un Servicio/Diálogo)
        private void HandleNewGameRequest()
        {
            // La apertura de GameEditWindow se maneja en el Code-behind (o un Servicio de Diálogo)
            System.Windows.MessageBox.Show("Comando de creación de juego recibido por el VM padre. Se notifica a la vista para abrir el diálogo.");
        }

        private void HandleLogoutRequest()
        {
            Session.JwtToken = string.Empty;
            Session.UserEmail = string.Empty;
            NavbarVM.UserEmail = string.Empty; // Actualizar la UI del hijo

            // La navegación (abrir LoginWindow) se maneja en el Code-behind
            System.Windows.MessageBox.Show("Sesión cerrada. Notificando a la vista para el cambio de pantalla.");
        }

        private void GameSelected(object? parameter)
        {
            // Similar: Notificar a la Vista para abrir la ventana de edición.
            if (parameter is GameDTOWithId game)
            {
                System.Windows.MessageBox.Show($"Juego {game.Title} seleccionado. Notificando a la vista para abrir el diálogo de edición.");
            }
        }
    }
}