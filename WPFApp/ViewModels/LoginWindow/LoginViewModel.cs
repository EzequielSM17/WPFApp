using Api;
using DTOs;
using Service.Interfaces;
using System.Windows.Controls; // Necesario para PasswordBox
using System.Windows.Input;
using Utils;


namespace ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly AuthApiClient _api;
        private readonly INavigationService _navigationService;

        // Propiedad para el Email (Bindable)
        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        // Propiedad para mensajes de error (Bindable)
        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public LoginViewModel(INavigationService navigationService)
        {
            _api = new AuthApiClient();
            _navigationService = navigationService;

            LoginCommand = new AsyncRelayCommand(LoginAsync);
            NavigateToRegisterCommand = new RelayCommand(_ => _navigationService.NavigateToRegister());
        }

        private async Task LoginAsync(object? parameter)
        {
            ErrorMessage = string.Empty;

            // 1. Obtener la contraseña del parámetro
            var passwordBox = parameter as PasswordBox;
            var password = passwordBox?.Password ?? string.Empty;

            // 2. Validaciones básicas
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Introduce email y contraseña.";
                return;
            }

            try
            {
                // 3. Llamada a la API
                LoginDTO dto = new LoginDTO { Email = Email, Password = password };
                var token = await _api.LoginAsync(dto);

                if (string.IsNullOrWhiteSpace(token))
                {
                    ErrorMessage = "Credenciales incorrectas.";
                    return;
                }

                // 4. Guardar sesión
                Session.UserEmail = Email;
                ApiClientBase.SetJwtToken(token);

                // 5. Navegar al Main usando el servicio
                _navigationService.NavigateToMain();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error de conexión: {ex.Message}";
            }
        }
    }
}