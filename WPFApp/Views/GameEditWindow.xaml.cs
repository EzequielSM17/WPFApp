using System.Windows;
using DTOs;
using WPFApp.ViewModels;

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
        }
    }
}