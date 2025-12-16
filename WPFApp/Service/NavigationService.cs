using DTOs;
using Service.Interfaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

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

        public void NavigateToLogout(object? sender, EventArgs e)
        {

            var login = new LoginWindow();
            login.Show();
            _owner.Close();
        }

        public bool? ShowEditGameDialog(GameDTOWithId game, bool isNew)
        {
            var window = new GameEditWindow(game, isNew);
            return window.ShowDialog();
        }
        public void ShowError(string message, string title = "Error")
        {
            ShowCustomToast(message, Colors.Crimson); 
        }

        public void ShowMessage(string message, string title = "Info")
        {
            ShowCustomToast(message, Colors.DarkOrange); 
        }

        
        private void ShowCustomToast(string message, Color color)
        {
            
            Application.Current.Dispatcher.Invoke(async () =>
            {
                
                var toastWindow = new Window
                {
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true,
                    Background = Brushes.Transparent,
                    Topmost = true,
                    ShowInTaskbar = false,
                    Width = 350,
                    Height = 140,
                    Title = "Toast",
                    Focusable = false
                };

                
                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)), 
                    BorderBrush = new SolidColorBrush(color),
                    BorderThickness = new Thickness(0, 0, 0, 2), 
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(15),
                    Margin = new Thickness(10)
                };

                
                border.Effect = new DropShadowEffect { BlurRadius = 10, ShadowDepth = 3, Opacity = 0.5 };

                var textBlock = new TextBlock
                {
                    Text = message,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,
                    TextWrapping = TextWrapping.Wrap
                };

                border.Child = textBlock;
                toastWindow.Content = border;
                var workArea = SystemParameters.WorkArea;


                toastWindow.Left = workArea.Right - toastWindow.Width - 20;

                toastWindow.Top = workArea.Top + 20;


                toastWindow.Show();

                
                await Task.Delay(6000);

                
                toastWindow.Close();
            });
        }
    }
}
