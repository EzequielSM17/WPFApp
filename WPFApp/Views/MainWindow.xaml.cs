using DTOs;
using Service;
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
            var navigationService = new NavigationService(this);
            DataContext = viewModel;

            InitializeComponent();

            
            ViewModel.SidebarVM.RequestNewGameDialog += Sidebar_RequestNewGameDialog;
            ViewModel.SidebarVM.RequestLogout += navigationService.NavigateToLogout;

         
            ViewModel.GameSelectedCommand = new RelayCommand(HandleGameSelected);
        }


        private async void Sidebar_RequestNewGameDialog(object? sender, EventArgs e)
        {
           
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

           
            if (result == true)
            {
               
                await ViewModel.LoadGamesAsync(ViewModel.PaginationVM.CurrentPage);
            }
        }


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

        
    }
}