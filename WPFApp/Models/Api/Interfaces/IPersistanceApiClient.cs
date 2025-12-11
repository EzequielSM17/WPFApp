

namespace Api.Interfaces
{
    public interface IPersistenceApiClient
    {
        Task<string> GetPersistenceModeAsync();
        Task<bool> SetPersistenceAsync(string mode);
    }
}
