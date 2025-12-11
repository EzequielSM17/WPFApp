using Api;
using DTOs;
using Service.Interfaces;
using System.Windows.Controls; 
using System.Windows.Input;
using ViewModels;

namespace WPFApp.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private readonly AuthApiClient _api;
        private readonly INavigationService _navigationService;

        // Propiedades enlazables
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        public RegisterViewModel(INavigationService navigationService)
        {
            _api = new AuthApiClient();
            _navigationService = navigationService;

            RegisterCommand = new AsyncRelayCommand(RegisterAsync);
            NavigateToLoginCommand = new RelayCommand(_ => _navigationService.NavigateToLogin());
        }

        private async Task RegisterAsync(object? parameter)
        {
            ErrorMessage = string.Empty;

            // 1. Obtener contraseña del parámetro (PasswordBox)
            var passwordBox = parameter as PasswordBox;
            var password = passwordBox?.Password.Trim() ?? string.Empty;

            // 2. Validaciones
            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Rellena todos los campos.";
                return;
            }

            try
            {
                // 3. Crear DTO
                var dto = new RegisterDTO
                {
                    Name = Name,
                    Email = Email,
                    Password = password
                };

                // 4. Llamar API
                var ok = await _api.RegisterAsync(dto);

                if (ok)
                {
                    // Éxito: Navegar al Login
                    // Opcional: Podrías usar un servicio de diálogos para mostrar el MessageBox
                    // System.Windows.MessageBox.Show("Usuario registrado..."); 

                    _navigationService.NavigateToLogin();
                }
                else
                {
                    ErrorMessage = "No se pudo registrar el usuario (verifica si el email ya existe).";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error de conexión: {ex.Message}";
            }
        }
    }
}