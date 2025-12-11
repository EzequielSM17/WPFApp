using System.Net.Http.Json;
using Api.Interfaces;
using DTOs;

namespace Api
{
    public class PersistenceApiClient : ApiClientBase, IPersistenceApiClient
    {
        public PersistenceApiClient() : base() { }

        public async Task<string> GetPersistenceModeAsync()
        {
            try
            {
                var response = await Http.GetAsync(Config.PersistenceEndpoint);
                if (!response.IsSuccessStatusCode)
                    return "DESCONOCIDO";

                var dto = await response.Content.ReadFromJsonAsync<PersistenceModeDTO>(JsonOptions);
                return dto?.Mode ?? "DESCONOCIDO";
            }
            catch
            {
                return "DESCONOCIDO";
            }
        }

        public async Task<bool> SetPersistenceAsync(string mode)
        {
            var url = $"{Config.PersistenceEndpoint}?mode={mode}";
            var response = await Http.PostAsync(url, null);
            return response.IsSuccessStatusCode;
        }
    }
}
