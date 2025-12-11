using Service;
using System.Windows;
using WPFApp.ViewModels;

namespace WPFApp
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();

            // 1. Crear servicio de navegación
            var navigationService = new NavigationService(this);

            // 2. Inyectar en el ViewModel
            DataContext = new RegisterViewModel(navigationService);
        }
    }
}