using DTOs;
using System.Windows;
using System.Windows.Input;
using ViewModels;

namespace WPFApp
{
    public partial class GameEditWindow : Window
    {
        public GameEditWindow(GameDTOWithId game, bool isNew)
        {
            InitializeComponent();

            // Creamos el ViewModel pasándole:
            // 1. El juego
            // 2. El flag de nuevo
            // 3. Una acción anónima que permite al VM cerrar esta ventana y establecer el resultado
            var viewModel = new GameEditViewModel(game, isNew, (result) =>
            {
                this.DialogResult = result;
                this.Close();
            });

            DataContext = viewModel;
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

    }
}