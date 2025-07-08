using LalaHealthCare.DataAccess.Models;
using static LalaHealthCare.DataAccess.Models.AuthDtos;

namespace LalaHealthCare.DataAccess.Repositories;

public interface IAuthenticationRepository
{
    Task<LoginResponse> AuthenticateAsync(string username, string password);
    Task<User?> GetUserByIdAsync(string userId);
}
