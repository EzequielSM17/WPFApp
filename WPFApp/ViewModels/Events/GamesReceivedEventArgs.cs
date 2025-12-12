using DTOs;

namespace Events
{
    public class GamesReceivedEventArgs : EventArgs
    {
        public PagedDTO<GameDTOWithId> Games { get; }

        public GamesReceivedEventArgs(PagedDTO<GameDTOWithId> games)
        {
            Games = games;
        }
    }
}
