using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Utils;

namespace Api
{
    public abstract class ApiClientBase
    {
        protected static readonly ApiConfig Config;
        protected static readonly JsonSerializerOptions JsonOptions;
        protected static readonly HttpClient Http;
        protected static string? JwtToken;

        static ApiClientBase()
        {
            var env = EnvLoader.LoadEnvFile();

            Config = new ApiConfig
            {
                BaseUrl = env.GetValueOrDefault("API_BASE_URL", "http://localhost:5001"),
                PersistenceEndpoint = env.GetValueOrDefault("API_PERSISTENCE", "/api/persistence"),
                GamesEndpoint = env.GetValueOrDefault("API_GAMES", "/api/games"),
                AuthLoginEndpoint = env.GetValueOrDefault("API_AUTH_LOGIN", "/api/auth/login"),
                AuthRegisterEndpoint = env.GetValueOrDefault("API_AUTH_REGISTER", "/api/auth/register")
            };

            JsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            Http = new HttpClient
            {
                BaseAddress = new Uri(Config.BaseUrl)
            };
        }

        protected ApiClientBase()
        {
            // no hacemos nada aquí, usamos el Http static
        }

        public static void SetJwtToken(string? token)
        {
            JwtToken = token;

            // limpiamos el header y lo volvemos a poner si hay token
            Http.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrWhiteSpace(token))
            {
                Http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

    }
}
