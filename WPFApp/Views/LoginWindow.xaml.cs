using Service;
using System.Windows;
using ViewModels;

namespace WPFApp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // Configurar dependencias
            var navigationService = new NavigationService(this);

            // Asignar ViewModel
            DataContext = new LoginViewModel(navigationService);
        }
    }
}