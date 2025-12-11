using System.Windows;
using Service.Interfaces;
using WPFApp;

namespace Service
{
    public class NavigationService : INavigationService
    {
        private readonly Window _owner;

        public NavigationService(Window owner)
        {
            _owner = owner;
        }

        public void NavigateToLogin()
        {
            var login = new LoginWindow();
            login.Show();
            _owner.Close();
        }

        public void NavigateToRegister()
        {
            var reg = new RegisterWindow(); // Asumiendo que existe
            reg.Show();
            _owner.Close();
        }

        public void NavigateToMain()
        {
            var main = new MainWindow();
            main.Show();
            _owner.Close();
        }
    }
}
