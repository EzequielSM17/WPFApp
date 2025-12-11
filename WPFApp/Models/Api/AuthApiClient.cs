using System.Text.Json;
using System.Net.Http.Json;
using Api.Interfaces;
using DTOs;

namespace Api
{
    public class AuthApiClient : ApiClientBase, IAuthApiClient
    {
        public AuthApiClient() : base() { }

        public async Task<bool> RegisterAsync(RegisterDTO dto)
        {
            var response = await Http.PostAsJsonAsync(Config.AuthRegisterEndpoint, dto, JsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<string?> LoginAsync(LoginDTO dto)
        {
            var response = await Http.PostAsJsonAsync(Config.AuthLoginEndpoint, dto, JsonOptions);
            if (!response.IsSuccessStatusCode)
                return null;

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            // Asumimos que el backend devuelve { token: "xxx" }
            if (doc.RootElement.TryGetProperty("token", out var tokenProp))
            {
                return tokenProp.GetString();
            }

            return null;
        }
    }
}
