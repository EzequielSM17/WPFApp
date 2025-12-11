using DTOs;

namespace Api.Interfaces
{
    public interface IAuthApiClient
    {
        Task<bool> RegisterAsync(RegisterDTO dto);
        Task<string?> LoginAsync(LoginDTO dto);
    }
}
