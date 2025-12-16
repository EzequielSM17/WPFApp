using Api;
using DTOs;
using Service.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Utils;

namespace ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly GamesApiClient _gamesApi;
        private readonly INavigationService _navigationService;
        private readonly IDispatcherService _dispatcherService;
        private readonly int _pageSize = 30;

        public NavbarViewModel NavbarVM { get; }
        public SidebarViewModel SidebarVM { get; }
        public PaginationViewModel PaginationVM { get; }

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

        public MainWindowViewModel(INavigationService navigationService, IDispatcherService dispatcherService)
        {
            _navigationService = navigationService;
            _dispatcherService = dispatcherService;
            _gamesApi = new GamesApiClient();

            NavbarVM = new NavbarViewModel(_navigationService,_dispatcherService);
            SidebarVM = new SidebarViewModel();
            PaginationVM = new PaginationViewModel();

            NavbarVM.GamesReceived += (sender, e) =>
            {
                _dispatcherService.Invoke(() =>
                {
                    Games.Clear();
                    if (e.PagedGames.Items != null)
                    {
                        foreach (var game in e.PagedGames.Items)
                        {
                            Games.Add(game);
                        }
                    }
                    PaginationVM.CurrentPage = e.PagedGames.Page;
                    PaginationVM.TotalPages = e.PagedGames.TotalPages;
                    FilterTitle = e.PagedGames.FilterTitle;
                    FilterCategory = e.PagedGames.FilterCategory;
                    FilterFromDate = e.PagedGames.FilterFromDate;
                    FilterToDate = e.PagedGames.FilterToDate;
                });
            };

            NavbarVM.PersistenceModeChanged += async (s, e) =>
            {
                await LoadGamesAsync(PaginationVM.CurrentPage);
            };
            NavbarVM.UserEmail = Session.UserEmail;

            SidebarVM.RequestNewGameDialog += (s, e) => HandleNewGameRequest();
            SidebarVM.RequestLogout += (s, e) => HandleLogoutRequest();

            PaginationVM.PageChangeRequested += async (s, page) =>
            {
                await LoadGamesAsync(page);
            };

            _ = LoadGamesAsync(1);
            SearchCommand = new AsyncRelayCommand(_ => LoadGamesAsync(1));
            ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);
            GameSelectedCommand = new RelayCommand(GameSelected);
        }

        private async Task ClearFiltersAsync(object? parameter)
        {
            FilterTitle = string.Empty;
            FilterCategory = string.Empty;
            FilterFromDate = null;
            FilterToDate = null;

            await LoadGamesAsync(1);
        }

        public async Task LoadGamesAsync(int page)
        {
            try
            {
                var paged = await _gamesApi.GetGamesAsync(page, _pageSize, FilterTitle, FilterCategory, FilterFromDate, FilterToDate);
                Games.Clear();

                if (paged?.Items != null)
                {
                    foreach (var g in paged.Items)
                        Games.Add(g);

                    PaginationVM.TotalPages = paged.TotalPages;
                    PaginationVM.CurrentPage = paged.Page;
                }
                else
                {
                    PaginationVM.TotalPages = 1;
                    PaginationVM.CurrentPage = 1;
                }
            }
            catch (Exception ex)
            {
                _navigationService.ShowError($"Error al cargar juegos: {ex.Message}");
            }
        }

        private void HandleNewGameRequest()
        {
            var newGame = new GameDTOWithId { ReleaseDate = DateTime.Now, IsActive = true };
            var result = _navigationService.ShowEditGameDialog(newGame, true);

            if (result == true)
            {
                _navigationService.ShowMessage("Juego creado correctamente", "Éxito");
                _ = LoadGamesAsync(1);
            }
        }

        private void HandleLogoutRequest()
        {
            Session.JwtToken = string.Empty;
            Session.UserEmail = string.Empty;
            NavbarVM.UserEmail = string.Empty;

            _navigationService.NavigateToLogin();
        }

        private void GameSelected(object? parameter)
        {
            if (parameter is GameDTOWithId game)
            {
                var result = _navigationService.ShowEditGameDialog(game, false);

                if (result == true)
                {
                    _navigationService.ShowMessage("Juego actualizado correctamente", "Éxito");
                    _ = LoadGamesAsync(PaginationVM.CurrentPage);
                }
            }
        }
    }
}