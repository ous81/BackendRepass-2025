using BackendRetake_2025.DTOs;
using BackendRetake_2025.Models;

namespace BackendRetake_2025.Services;
public interface IAuthService
{
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<bool> ValidateUserAsync(string email, string password);
    Task<User?> GetUserByEmailAsync(string email);
    string GenerateJwtToken(User user);
}
