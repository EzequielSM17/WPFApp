using DTOs;

namespace Api.Interfaces
{
    public interface IGamesApiClient
    {
        Task<PagedDTO<GameDTOWithId>?> GetGamesAsync(
            int page,
            int pageSize,
            string? title,
            string? category,
            DateTime? fromDate,
            DateTime? toDate);

        Task<GameDTOWithId?> GetGameByIdAsync(int id);
        Task<GameDTOWithId?> CreateGameAsync(GameDTO game);
        Task<bool> UpdateGameAsync(int id, GameDTO game);
        Task<bool> DeleteGameAsync(int id);
    }
}
