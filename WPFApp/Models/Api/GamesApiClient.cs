using Api.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;
using DTOs;

namespace Api
{
    public class GamesApiClient : ApiClientBase, IGamesApiClient
    {
        public GamesApiClient() : base() { }

        public async Task<PagedDTO<GameDTOWithId>?> GetGamesAsync(
            int page,
            int pageSize,
            string? title,
            string? category,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(title))
                query.Add($"title={Uri.EscapeDataString(title)}");

            if (!string.IsNullOrWhiteSpace(category))
                query.Add($"category={Uri.EscapeDataString(category)}");

            if (fromDate.HasValue)
                query.Add($"fromDate={fromDate:yyyy-MM-dd}");

            if (toDate.HasValue)
                query.Add($"toDate={toDate:yyyy-MM-dd}");

            var url = Config.GamesEndpoint;
            if (query.Count > 0)
                url += "?" + string.Join("&", query);

            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<PagedDTO<GameDTOWithId>>(stream, JsonOptions);
        }

        public async Task<GameDTOWithId?> GetGameByIdAsync(int id)
        {
            var response = await Http.GetAsync($"{Config.GamesEndpoint}/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<GameDTOWithId>(stream, JsonOptions);
        }

        public async Task<GameDTOWithId?> CreateGameAsync(GameDTO game)
        {
            var response = await Http.PostAsJsonAsync(Config.GamesEndpoint, game, JsonOptions);
            if (!response.IsSuccessStatusCode)
                return null;

            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<GameDTOWithId>(stream, JsonOptions);
        }

        public async Task<bool> UpdateGameAsync(int id, GameDTO game)
        {
            // Cambia a PATCH si tu backend usa PATCH
            var response = await Http.PatchAsJsonAsync($"{Config.GamesEndpoint}/{id}", game, JsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteGameAsync(int id)
        {
            var response = await Http.DeleteAsync($"{Config.GamesEndpoint}/{id}");
            return response.IsSuccessStatusCode;
        }
    }

}
