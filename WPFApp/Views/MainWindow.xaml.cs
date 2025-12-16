using DTOs;
using Service;
using System.Windows;
using System.Windows.Input;
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
            var viewModel = new MainWindowViewModel(new NavigationService(this), new DispacherService());
            var navigationService = new NavigationService(this);
            DataContext = viewModel;

            InitializeComponent();

            
            ViewModel.SidebarVM.RequestNewGameDialog += Sidebar_RequestNewGameDialog;
            ViewModel.SidebarVM.RequestLogout += navigationService.NavigateToLogout;

         
            ViewModel.GameSelectedCommand = new RelayCommand(HandleGameSelected);
            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow, OnCanResizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));
        }

        // --- MÉTODOS AUXILIARES PARA QUE FUNCIONE LA BARRA ---
        private void OnCloseWindow(object target, ExecutedRoutedEventArgs e) => SystemCommands.CloseWindow(this);
        private void OnMaximizeWindow(object target, ExecutedRoutedEventArgs e) => SystemCommands.MaximizeWindow(this);
        private void OnMinimizeWindow(object target, ExecutedRoutedEventArgs e) => SystemCommands.MinimizeWindow(this);
        private void OnRestoreWindow(object target, ExecutedRoutedEventArgs e) => SystemCommands.RestoreWindow(this);

        private void OnCanResizeWindow(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip;
        private void OnCanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = this.ResizeMode != ResizeMode.NoResize;


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
                var result = editWindow.ShowDialog();
                // Opcional: refrescar lista tras edición
                await ViewModel.LoadGamesAsync(ViewModel.PaginationVM.CurrentPage);
            }
        }

        
    }
}