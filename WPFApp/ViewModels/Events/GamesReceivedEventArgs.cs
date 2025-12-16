using DTOs;

namespace Events
{
    public class GamesReceivedEventArgs : EventArgs
    {
        public PagedDTO<GameDTOWithId> PagedGames { get; }

        public GamesReceivedEventArgs(PagedDTO<GameDTOWithId> games)
        {
            PagedGames = games;
        }
    }
}
